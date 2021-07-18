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
    [SerializeField] private Track[] tracks;
    private List<Cube> _cubes;
    private int currentSegment;
    private Track chosenTrack;
    private Level level;
    private bool paramsChosen = true;
    public bool DeleteMode { private get; set; } = false;
    public bool AddMode { private get; set; } = false;
    private void Awake()
    {
        _cubes = FindObjectsOfType<Cube>().ToList();
        int i = 0;
        foreach (Track track in tracks)
        {
            GameObject createdButton = Instantiate(buttonPrefab, content.transform);
            createdButton.GetComponent<RectTransform>().localPosition -= new Vector3(0f, 100 * i, 0f);
            createdButton.GetComponentsInChildren<Text>()[0].text = track.name;
            createdButton.GetComponentsInChildren<Text>()[1].text = track.author;
            createdButton.GetComponentInChildren<Image>().sprite = track.sprite;
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
            foreach (Cube cube in _cubes)
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
            currentSegment = Mathf.FloorToInt(audioSource.time / (1 / (float)segmentMulti));
            currentSegment = currentSegment > level.Segments ? currentSegment - 1 : currentSegment;
            currentSegment = currentSegment < 0 ? 0 : currentSegment;
            segmentShow.text = $"Segment: {currentSegment} / {level.Segments}";
            slider.value = audioSource.time / audioSource.clip.length;
        }
    }
    public void ClickedCube(Cube chosenCube)
    {
        if (chosenCube.CurrentColor == Cube.CubeColor.gray && paramsChosen && AddMode)
        {
            paramsChosen = false;
            chosenCube.CurrentColor = Cube.CubeColor.blue;
            level.cubeSegments[currentSegment].elements.Add(new Level.Element());
            Vector2 pos = chosenCube.transform.position;
            level.cubeSegments[currentSegment].elements.Last().position = pos;
            rotations.SetActive(true);
        }
        if (chosenCube.CurrentColor != Cube.CubeColor.gray && DeleteMode)
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
    public void MoveSecond(int sec)
    {
        audioSource.time += sec;
    }
    public void MoveSegment(int seg)
    {
        audioSource.time += 1 / (float)segmentMulti * seg;
    }
    public void ChosenSong(Track track)
    {
        chosenTrack = track;
        audioSource.clip = chosenTrack.audio;
        level = new Level(chosenTrack, segmentMulti);
        choosingMenu.SetActive(false);
        editMenu.SetActive(true);
        cubes.SetActive(true);
    }
    public void Save()
    {
        string path;
#if UNITY_ANDROID && !UNITY_EDITOR
        path = Path.Combine(Application.persistentDataPath, "Levels.json");
#else
        path = Path.Combine(Application.dataPath, "Level.json");
#endif
        File.WriteAllText(path, JsonUtility.ToJson(level));
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
    public Level(Track track, int segMulti)
    {
        Segments = Mathf.FloorToInt(track.audio.length) * segMulti;
        Audio = track.audio;
        Author = track.author;
        Name = track.name;
        Sprite = track.sprite;
        cubeSegments = new CubeSegment[Segments];
        for (int i = 0; i < cubeSegments.Length; i++)
        {
            cubeSegments[i] = new CubeSegment();
        }
    }
    public AudioClip Audio;
    public string Author;
    public string Name;
    public Sprite Sprite;
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