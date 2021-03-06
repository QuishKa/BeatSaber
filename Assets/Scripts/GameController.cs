using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] private InputController inputController;
    [SerializeField] private GameObject levelOverview;
    [SerializeField] private GameObject recordsOverview;
    [SerializeField] private Slider levelSlider;
    [SerializeField] private GameObject songPrefab;
    [SerializeField] private GameObject recordPrefab;
    [SerializeField] private Cube cubePrefab;
    [SerializeField] private GameObject cubesContainer;
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject choosingMenu;
    [SerializeField] private GameObject scoreMenu;
    [SerializeField] private AudioSource mainAudio;
    [SerializeField] private AudioSource goodSliceAudio;
    [SerializeField] private AudioSource badSliceAudio;
    [SerializeField] private GameObject comboField;
    [SerializeField] private GameObject scoreField;
    [SerializeField] private GameObject multiField;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject lights;
    [SerializeField] private Track[] tracks;
    public List<Cube> ActiveCubes { get; private set; } = new List<Cube>();
    private float timeSinceLevelStarted;
    private Level currentLevel = null;
    private int currentSegment;
    private Text scoreFieldText;
    private Text comboFieldText;
    private Text multiFieldText;
    private Renderer[] lightRenderers;
    private int Score;
    private int Combo;
    private int ComboMulti;
    private int MissedCubes;
    private int TotalCubesSpawned;
    private bool levelIsPlaying = false;
    private string recordsPath;
    private Records records = new Records();
    private AudioClip backgroundMusic;
    void Start()
    {
        // initialize text fields and lights
        scoreFieldText = scoreField.GetComponentInChildren<Text>();
        comboFieldText = comboField.GetComponentsInChildren<Text>()[1];
        multiFieldText = multiField.GetComponentsInChildren<Text>()[1];
        lightRenderers = lights.GetComponentsInChildren<Renderer>();
        // read records
#if UNITY_ANDROID && !UNITY_EDITOR
        recordsPath = Path.Combine(Application.persistentDataPath, $"_Records.json");
#else
        recordsPath = Path.Combine(Application.dataPath, $"_Records.json");
#endif
        if (File.Exists(recordsPath))
            records = JsonUtility.FromJson<Records>(File.ReadAllText(recordsPath));
        // get last played level music
        if (PlayerPrefs.HasKey("LastLevel"))
        {
            string name = PlayerPrefs.GetString("LastLevel");
            string path;
#if UNITY_ANDROID && !UNITY_EDITOR
            path = Path.Combine(Application.persistentDataPath, $"{name}.json");
#else
            path = Path.Combine(Application.dataPath, $"{name}.json");
#endif
            Level level = JsonUtility.FromJson<Level>(File.ReadAllText(path));
            backgroundMusic = tracks[level.index].audio;
            mainAudio.clip = backgroundMusic;
        }
        mainAudio.Play();
    }
    private void Update()
    {
        scoreFieldText.text = Score.ToString();
        comboFieldText.text = Combo.ToString();
        multiFieldText.text = ComboMulti.ToString();
        if (ComboMulti % 2 == 0) // change scene lights
        {
            foreach (Renderer renderer in lightRenderers)
            {
                renderer.material.SetColor("_Color", new Color(0.705882f, 0.741176f, 0.905882f));
                renderer.material.SetColor("_EmissionColor", new Color(0.705882f, 0.741176f, 0.905882f));
            }
        }
        else
        {
            foreach (Renderer renderer in lightRenderers)
            {
                renderer.material.SetColor("_Color", new Color(0.925490f, 0.623529f, 0.596078f));
                renderer.material.SetColor("_EmissionColor", new Color(0.749019f, 0f, 0f));
            }
        }
        if (currentLevel != null) // level slider control
        {
            levelSlider.value = mainAudio.time / mainAudio.clip.length;
        }
    }
    void FixedUpdate()
    {
        if (levelIsPlaying)
        {
            // reading level segments based on the time level starte playing
            float deltaTimeSinceLevelStarted = Time.fixedTime - timeSinceLevelStarted;
            int segment = Mathf.FloorToInt(deltaTimeSinceLevelStarted / (1 / (currentLevel.Segments / mainAudio.clip.length)));
            if (segment != currentSegment && segment < currentLevel.Segments) // spawn cubes
            {
                currentSegment = segment;
                SpawnCube();
            }
            for (int i = 0; i < ActiveCubes.Count; i++) // move cubes towards the player
            {
                ActiveCubes[i].gameObject.transform.position += new Vector3(0f, 0f, -0.35f);
                if (ActiveCubes[i].transform.position.z < -12) // delete missed cubes
                {
                    SliceCube(ActiveCubes[i]);
                    i--;
                }
            }
            if (!mainAudio.isPlaying && segment >= currentLevel.Segments) // finish level
            {
                SaveRecord();
                FinishLevel();
            }
        }
    }

    public void ButtonPressed(string pressedButton) // old idea of pressing buttons
    {
        switch (pressedButton)
        {
            case "edit":
                SceneManager.LoadScene(1);
                break;
            case "exit":
                Application.Quit();
                break;
        }
    }

    public void ReadLevelDirectory() // reads the list of levels and instantiates their starting buttons
    {
        foreach (Button button in levelOverview.GetComponentsInChildren<Button>())
        {
            Destroy(button.gameObject);
        }
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
            if (path == recordsPath) continue;
            Level newLevel = JsonUtility.FromJson<Level>(File.ReadAllText(path));
            GameObject createdButton = Instantiate(songPrefab, levelOverview.transform);
            createdButton.GetComponent<RectTransform>().localPosition -= new Vector3(0f, 100 * i, 0f);
            createdButton.GetComponentsInChildren<Text>()[0].text = newLevel.Name + $"  |  {newLevel.LevelName}";
            createdButton.GetComponentsInChildren<Text>()[1].text = newLevel.Author;
            createdButton.GetComponentsInChildren<Image>()[1].sprite = tracks[newLevel.index].sprite;
            createdButton.GetComponent<Button>().onClick.AddListener(() => LoadLevel(newLevel));
            i++;
        }
    }

    private void LoadLevel(Level level)
    {
        PlayerPrefs.SetString("LastLevel", level.LevelName);
        menu.SetActive(false);
        scoreField.SetActive(true);
        comboField.SetActive(true);
        multiField.SetActive(true);
        pauseMenu.SetActive(true);
        mainAudio.clip = tracks[level.index].audio;
        mainAudio.loop = false;
        currentSegment = -1;
        currentLevel = level;
        Score = 0;
        Combo = 0;
        MissedCubes = 0;
        TotalCubesSpawned = 0;
        mainAudio.Stop();
        mainAudio.PlayDelayed(2f);
        timeSinceLevelStarted = Time.fixedTime;
        levelIsPlaying = true;
    }

    private void FinishLevel()
    {
        menu.SetActive(true);
        scoreMenu.SetActive(true);
        choosingMenu.SetActive(false);
        scoreField.SetActive(false);
        comboField.SetActive(false);
        multiField.SetActive(false);
        pauseMenu.SetActive(false);
        foreach (Cube cube in ActiveCubes)
        {
            Destroy(cube.gameObject);
            MissedCubes++;
        }
        ActiveCubes.Clear();
        mainAudio.Stop();
        backgroundMusic = tracks[currentLevel.index].audio;
        scoreMenu.GetComponentsInChildren<Text>()[2].text = Score.ToString();
        Debug.Log(TotalCubesSpawned);
        Debug.Log(MissedCubes);
        char rating;
        if (MissedCubes <= TotalCubesSpawned / 25) rating = 'S';
        else if (MissedCubes <= TotalCubesSpawned / 10) rating = 'A';
        else if (MissedCubes <= TotalCubesSpawned / 5) rating = 'B';
        else if (MissedCubes <= TotalCubesSpawned / 3) rating = 'C';
        else if (MissedCubes <= TotalCubesSpawned / 2) rating = 'D';
        else rating = 'F';
        scoreMenu.GetComponentsInChildren<Text>()[3].text = rating.ToString();
        Score = 0;
        Combo = 0;
        ComboMulti = 0;
        currentLevel = null;
        levelIsPlaying = false;
        mainAudio.clip = backgroundMusic;
        mainAudio.loop = true;
        mainAudio.Play();
    }

    private void SaveRecord()
    {
        Records.Record record;
        if (records.records.Exists(record => record.levelName == currentLevel.LevelName))
        {
            record = records.records.Find(record => record.levelName == currentLevel.LevelName);
            if (record.score < Score)
                record.score = Score;
        }
        else
        {
            record = new Records.Record(Score, currentLevel);
            records.records.Add(record);
        }
        File.WriteAllText(recordsPath, JsonUtility.ToJson(records));
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        mainAudio.Pause();
        levelIsPlaying = false;
        inputController.gamePaused = true;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        LoadLevel(currentLevel);
        inputController.gamePaused = false;
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        mainAudio.Play();
        levelIsPlaying = true;
        inputController.gamePaused = false;
    }

    public void Quit()
    {
        Time.timeScale = 1f;
        SaveRecord();
        FinishLevel();
        inputController.gamePaused = false;
    }

    public void LoadRecords() // reads the list of records and shows them in records menu
    {
        foreach (Button button in recordsOverview.GetComponentsInChildren<Button>())
        {
            Destroy(button.gameObject);
        }
        int i = 0;
        foreach (Records.Record record in records.records)
        {
            GameObject createdButton = Instantiate(recordPrefab, recordsOverview.transform);
            createdButton.GetComponent<RectTransform>().localPosition -= new Vector3(0f, 100 * i, 0f);
            createdButton.GetComponentsInChildren<Text>()[0].text = record.songName + $"  |  {record.levelName}";
            createdButton.GetComponentsInChildren<Text>()[1].text = record.levelAuthor;
            createdButton.GetComponentsInChildren<Text>()[2].text = record.score.ToString();
            createdButton.GetComponentsInChildren<Image>()[1].sprite = tracks[record.index].sprite;
            i++;
        }
    }

    private void SpawnCube() // spawn all cubes of current segment
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
            TotalCubesSpawned++;
        }
    }

    public void SliceCube(Cube slicedCube, Cube.CubeOrientation orientation) // slicing cubes based on the player's movement
    {
        if (orientation == slicedCube.Orientation)
        {
            goodSliceAudio.pitch = Random.Range(0.9f, 1.1f);
            goodSliceAudio.PlayOneShot(goodSliceAudio.clip);
            ComboMulti = Mathf.Clamp(Mathf.FloorToInt(Combo / 10), 1, 8);
            Score += 100 * ComboMulti;
            Combo++;
        }
        else
        {
            Combo = 0;
            MissedCubes++;
            Debug.Log("orientation miss: " + orientation);
        }
        ActiveCubes.Remove(slicedCube);
        slicedCube.Destroyed();
        Destroy(slicedCube.gameObject);
    }
    private void SliceCube(Cube slicedCube) // for deleting missed cubes
    {
        Combo = 0;
        MissedCubes++;
        ActiveCubes.Remove(slicedCube);
        slicedCube.Destroyed();
        Destroy(slicedCube.gameObject);
        Debug.Log("missed cube");
    }
    [System.Serializable]
    public class Records
    {
        public Records()
        {
            records = new List<Record>();
        }
        [System.Serializable]
        public class Record
        {
            public Record(int score, Level level)
            {
                this.score = score;
                songName = level.Name;
                levelName = level.LevelName;
                levelAuthor = level.Author;
                index = level.index;
            }
            public int score;
            public string songName;
            public string levelName;
            public string levelAuthor;
            public int index;
        }
        public List<Record> records;
    }
    [System.Serializable]
    public struct Track
    {
        [SerializeField] public AudioClip audio;
        [SerializeField] public Sprite sprite;
    }
}
