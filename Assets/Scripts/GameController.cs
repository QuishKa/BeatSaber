using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject levelOverview;
    [SerializeField] private GameObject songPrefab;
    [SerializeField] private Cube cubePrefab;
    [SerializeField] private GameObject cubesContainer;
    [SerializeField] private GameObject menu;
    [SerializeField] private AudioSource mainAudio;
    [SerializeField] private AudioSource goodSliceAudio;
    [SerializeField] private AudioSource badSliceAudio;
    [SerializeField] private GameObject comboField;
    [SerializeField] private GameObject scoreField;
    public List<Cube> ActiveCubes { get; private set; } = new List<Cube>();
    private float timeSinceLevelStarted;
    private Level currentLevel;
    private int currentSegment;
    public int score { get; private set; }
    private int combo;
    private bool levelIsPlaying = false;
    void Start()
    {
        
    }
    private void Update()
    {
        comboField.GetComponent<Text>().text = combo.ToString();
        scoreField.GetComponent<Text>().text = score.ToString();
    }
    void FixedUpdate()
    {
        if (levelIsPlaying)
        {
            float deltaTimeSinceLevelStarted = Time.fixedTime - timeSinceLevelStarted;
            int segment = Mathf.FloorToInt(deltaTimeSinceLevelStarted / (1 / (currentLevel.Segments / mainAudio.clip.length)));
            if (segment != currentSegment)
            {
                currentSegment = segment;
                SpawnCube();
            }
            foreach (Cube cube in ActiveCubes)
            {
                cube.gameObject.transform.position += new Vector3(0f, 0f, -0.35f);
                if (cube.transform.position.z < -12)
                {
                    SliceCube(cube);
                }
            }
        }
    }

    public void ButtonPressed(string pressedButton)
    {
        switch (pressedButton)
        {
            case "edit":
                SceneManager.LoadScene(1);
                break;
            case "options":
                break;
            case "exit":
                Application.Quit();
                break;
        }
    }

    public void ReadLevelDirectory()
    {
        string directoryPath;
#if UNITY_ANDROID && !UNITY_EDITOR
        directoryPath = Application.persistentDataPath;
#else
        directoryPath = Application.dataPath;
#endif
        string[] paths = Directory.GetFiles(directoryPath, "*.json");
        int i = 0;
        foreach (string path in paths)
        {
            Level newLevel = JsonUtility.FromJson<Level>(File.ReadAllText(path));
            GameObject createdButton = Instantiate(songPrefab, levelOverview.transform);
            createdButton.GetComponent<RectTransform>().localPosition -= new Vector3(0f, 100 * i, 0f);
            createdButton.GetComponentsInChildren<Text>()[0].text = newLevel.Name;
            createdButton.GetComponentsInChildren<Text>()[1].text = newLevel.Author;
            createdButton.GetComponentInChildren<Image>().sprite = newLevel.Sprite;
            createdButton.GetComponent<Button>().onClick.AddListener(() => LoadLevel(newLevel));
            i++;
        }
    }

    private void LoadLevel(Level level)
    {
        menu.SetActive(false);
        mainAudio.clip = level.Audio;
        mainAudio.PlayDelayed(2f);
        timeSinceLevelStarted = Time.fixedTime;
        currentSegment = -1;
        currentLevel = level;
        score = 0;
        combo = 0;
        levelIsPlaying = true;
    }

    private void SpawnCube()
    {
        Level.CubeSegment segment = currentLevel.cubeSegments[currentSegment];
        foreach (Level.Element element in segment.elements)
        {
            Vector3 pos = new Vector3(element.position.x, element.position.y, 33f);
            Quaternion rotation = Quaternion.AngleAxis(90, Vector3.down) * Quaternion.AngleAxis(180 - 45 * (int)element.orientation, Vector3.right);
            Cube cube = Instantiate(cubePrefab, pos, rotation, cubesContainer.transform);
            cube.CurrentColor = element.color;
            cube.Orientation = element.orientation;
            ActiveCubes.Add(cube);
        }
    }

    public void SliceCube(Cube slicedCube, Cube.CubeOrientation orientation)
    {
        Debug.Log(slicedCube.CurrentColor);
        if (orientation == slicedCube.Orientation)
        {
            goodSliceAudio.pitch = Random.Range(0.9f, 1.1f);
            goodSliceAudio.PlayOneShot(goodSliceAudio.clip);
            score += 100 + 100 * Mathf.Clamp(Mathf.FloorToInt(combo / 10), 0, 16);
            combo++;
        }
        else
        {
            combo = 0;
        }
        ActiveCubes.Remove(slicedCube);
        Destroy(slicedCube.gameObject);
    }
    private void SliceCube(Cube slicedCube)
    {
        combo = 0;
        ActiveCubes.Remove(slicedCube);
        Destroy(slicedCube.gameObject);
    }
}
