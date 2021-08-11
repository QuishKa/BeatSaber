using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Editor : MonoBehaviour
{
    [SerializeField] private GameObject choosingMenu;
    [SerializeField] private GameObject editMenu;
    [SerializeField] private GameObject cubes;
    [SerializeField] private GameObject rotations;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject content;
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Cube spawns per second.")]
    [SerializeField] private int segmentMulti;
    [SerializeField] private Text segmentShow;
    [SerializeField] private Slider slider;
    [SerializeField] private InputField lvlName;
    [SerializeField] private List<Track> tracks;
    private List<Cube> _cubes;
    private int currentSegment;
    private Track chosenTrack;
    private Level level;
    private bool paramsChosen = true;
    public bool DeleteMode { private get; set; } = false;
    public bool AddMode { private get; set; } = false;
    private void Awake() // load all awailable songs
    {
        _cubes = FindObjectsOfType<Cube>().ToList();
        int i = 0;
        foreach (Track track in tracks)
        {
            GameObject createdButton = Instantiate(buttonPrefab, content.transform);
            createdButton.GetComponent<RectTransform>().localPosition -= new Vector3(0f, 100 * i, 0f);
            createdButton.GetComponentsInChildren<Text>()[0].text = track.name;
            createdButton.GetComponentsInChildren<Text>()[1].text = track.author;
            createdButton.GetComponentsInChildren<Image>()[1].sprite = track.sprite;
            createdButton.GetComponent<Button>().onClick.AddListener(() => ChosenSong(track));
            i++;
        }
        choosingMenu.SetActive(true);
        editMenu.SetActive(false);
        cubes.SetActive(false);
        rotations.SetActive(false);
    }
    private void Update()
    {
        if (editMenu.activeInHierarchy)
        {
            // counts which segment we're currently in based on the song time
            currentSegment = Mathf.FloorToInt(audioSource.time / (1 / (float)segmentMulti));
            currentSegment = currentSegment >= level.Segments ? currentSegment - 1 : currentSegment;
            currentSegment = currentSegment < 0 ? 0 : currentSegment;
            segmentShow.text = $"Segment: {currentSegment} / {level.Segments - 1}";
            foreach (Cube cube in _cubes) // showing placed cubes
            {
                Level.CubeSegment cubeSegment = level.cubeSegments[currentSegment];
                if (cubeSegment.elements.Exists(elem => elem.position == (Vector2)cube.transform.position))
                {
                    Level.Element edited = cubeSegment.elements.First(elem => elem.position == (Vector2)cube.transform.position);
                    cube.CurrentColor = edited.color;
                    Quaternion rotation = Quaternion.AngleAxis(180 - 45 * (int)edited.orientation, Vector3.right);
                    cube.transform.rotation = cube.DefaultRotation * rotation;
                }
                else
                {
                    cube.CurrentColor = Cube.CubeColor.gray;
                    cube.transform.rotation = cube.DefaultRotation;
                }
            }
            slider.value = audioSource.time / audioSource.clip.length;
        }
    }
    public void ClickedCube(Cube chosenCube)
    {
        if (chosenCube.CurrentColor == Cube.CubeColor.gray && paramsChosen && AddMode) // add cube on grey position
        {
            paramsChosen = false;
            chosenCube.CurrentColor = Cube.CubeColor.blue;
            level.cubeSegments[currentSegment].elements.Add(new Level.Element());
            Vector2 pos = chosenCube.transform.position;
            level.cubeSegments[currentSegment].elements.Last().position = pos;
            rotations.SetActive(true);
        }
        if (chosenCube.CurrentColor != Cube.CubeColor.gray && DeleteMode) // delete cube from colored position
        {
            Level.Element deleted = level.cubeSegments[currentSegment].elements.First(elem => elem.position == (Vector2)chosenCube.transform.position);
            level.cubeSegments[currentSegment].elements.Remove(deleted);
            chosenCube.CurrentColor = Cube.CubeColor.gray;
            chosenCube.transform.rotation = chosenCube.DefaultRotation;
            DeleteMode = false;
            editMenu.SetActive(true);
        }
    }
    public void SetRotation(int orientation)
    {
        level.cubeSegments[currentSegment].elements.Last().orientation = (Cube.CubeOrientation)orientation;
    }
    public void SetColor(int color)
    {
        level.cubeSegments[currentSegment].elements.Last().color = (Cube.CubeColor)color;
        paramsChosen = true;
        AddMode = false;
    }
    public void MoveSlider()
    {
        audioSource.time = slider.value * audioSource.clip.length;
    }
    public void MoveSecond(int sec) // move by second
    {
        audioSource.time += sec;
    }
    public void MoveSegment(int seg) // move by segment
    {
        audioSource.time += 1 / (float)segmentMulti * seg;
    }
    public void ChosenSong(Track track) // load edit mode of chosen song
    {
        chosenTrack = track;
        int index = tracks.IndexOf(track);
        audioSource.clip = chosenTrack.audio;
        level = new Level(chosenTrack, segmentMulti, index);
        choosingMenu.SetActive(false);
        editMenu.SetActive(true);
        cubes.SetActive(true);
    }
    public void Save() // save track
    {
        string path;
#if UNITY_ANDROID && !UNITY_EDITOR
        path = Path.Combine(Application.persistentDataPath, $"{lvlName.textComponent.text}.json");
#else
        path = Path.Combine(Application.dataPath, $"{lvlName.textComponent.text}.json");
#endif
        level.LevelName = lvlName.textComponent.text;
        PlayerPrefs.SetString("LastLevel", level.LevelName);
        File.WriteAllText(path, JsonUtility.ToJson(level));
        choosingMenu.SetActive(true);
        editMenu.SetActive(false);
        cubes.SetActive(false);
    }
    public List<Cube> GetCubes() => _cubes;
    public void Back()
    {
        SceneManager.LoadScene(0);
    }
}
[System.Serializable]
public struct Track
{
    [SerializeField] public AudioClip audio;
    [SerializeField] public string author;
    [SerializeField] public string name;
    [SerializeField] public Sprite sprite;
}
[System.Serializable]
public class Level
{
    public Level(Track track, int segMulti, int id)
    {
        Segments = Mathf.FloorToInt(track.audio.length) * segMulti;
        index = id;
        Author = track.author;
        Name = track.name;
        cubeSegments = new CubeSegment[Segments];
        for (int i = 0; i < cubeSegments.Length; i++)
        {
            cubeSegments[i] = new CubeSegment();
        }
    }
    public int index;
    public string LevelName;
    public string Author;
    public string Name;
    public int Segments;
    public CubeSegment[] cubeSegments;
    [System.Serializable]
    public class Element
    {
        public Vector2 position;
        public Cube.CubeOrientation orientation;
        public Cube.CubeColor color;
    }
    [System.Serializable]
    public class CubeSegment
    {
        public CubeSegment()
        {
            elements = new List<Element>();
        }
        public List<Element> elements;
    }
}