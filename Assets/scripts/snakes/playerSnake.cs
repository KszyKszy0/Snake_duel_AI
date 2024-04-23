using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerSnake : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector2 lastDir;
    public Vector2 dir;


    public List<GameObject> snakeBody = new List<GameObject>();
    [SerializeField]
    public List<int> segmentsPlacement;


    public Sprite head;
    public Sprite body;
    public Sprite tail;
    public Sprite turn;


    public GameObject segment;
    public gameManagement GM;
    public GameEnviroment instance;


    public int start;


    void Start()
    {
        var i = Instantiate(segment, transform.position, Quaternion.identity);
        snakeBody.Add(i);
        i.GetComponent<SpriteRenderer>().sprite = head;
        segmentsPlacement.Add(start);
        i = Instantiate(segment, transform.position, Quaternion.identity);
        snakeBody.Add(i);
        i.GetComponent<SpriteRenderer>().sprite = body;
        i = Instantiate(segment, transform.position, Quaternion.identity);
        segmentsPlacement.Add(start);
        snakeBody.Add(i);
        i.GetComponent<SpriteRenderer>().sprite = tail;
        segmentsPlacement.Add(start);
        GM = GameObject.Find("GameManager").GetComponent<gameManagement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) && lastDir != Vector2.down)
            {
                dir = Vector2.up;
                snakeBody[0].transform.eulerAngles = new Vector3(0, 0, 0);

            }
            if ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) && lastDir != Vector2.up)
            {
                dir = Vector2.down;
                snakeBody[0].transform.eulerAngles = new Vector3(0, 0, 180);
            }
            if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) && lastDir != Vector2.right)
            {
                dir = Vector2.left;
                snakeBody[0].transform.eulerAngles = new Vector3(0, 0, 90);
            }
            if ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) && lastDir != Vector2.left)
            {
                dir = Vector2.right;
                snakeBody[0].transform.eulerAngles = new Vector3(0, 0, 270);
            }
            move();
        }
    }
    public void move()
    {
        int tailId = segmentsPlacement[segmentsPlacement.Count - 1];
        int row = tailId / 100;
        int col = tailId % 100;
        instance.map[row - 1, col - 1].isSnake = false;
        instance.map[row - 1, col - 1].isTail = false;
        int playerChangeTail = segmentsPlacement[segmentsPlacement.Count - 2];
        int playerRowChange = playerChangeTail / 100;
        int playerColChange = playerChangeTail % 100;
        instance.map[playerRowChange - 1, playerColChange - 1].isTail = true;
        if (snakeBody[snakeBody.Count - 2].GetComponent<SpriteRenderer>().sprite == turn)
        {
            snakeBody[snakeBody.Count - 1].transform.position = snakeBody[snakeBody.Count - 2].transform.position;
            snakeBody[snakeBody.Count - 1].transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(snakeBody[snakeBody.Count - 3].transform.position.y - snakeBody[snakeBody.Count - 1].transform.position.y, snakeBody[snakeBody.Count - 3].transform.position.x - snakeBody[snakeBody.Count - 1].transform.position.x) * Mathf.Rad2Deg - 90f);
        }
        else
        {
            snakeBody[snakeBody.Count - 1].transform.position = snakeBody[snakeBody.Count - 2].transform.position;        // ogółem to początek po prostu daje że miejsce z ogonem jest puste
            snakeBody[snakeBody.Count - 1].transform.rotation = snakeBody[snakeBody.Count - 2].transform.rotation;        // a tutaj to jeżeli przed ogonem jest skręt to po prostu robi się ten dziwny obrót 
        }
        for (int i = snakeBody.Count - 2; i >= 1; i--)
        {
            snakeBody[i].transform.position = snakeBody[i - 1].transform.position;                                         //w pętli po prostu ustawia się po kolei pozycje i spin dla wszystkich 
            snakeBody[i].transform.rotation = snakeBody[i - 1].transform.rotation;                                         //a zaraz po tym w drugiej pętli ustawia się grafiki na poprzednie
        }
        for (int i = snakeBody.Count - 2; i >= 2; i--)
        {
            snakeBody[i].GetComponent<SpriteRenderer>().sprite = snakeBody[i - 1].GetComponent<SpriteRenderer>().sprite;
        }
        for (int i = segmentsPlacement.Count - 1; i >= 1; i--)
        {
            segmentsPlacement[i] = segmentsPlacement[i - 1];
        }
        if (dir != lastDir)
        {
            float angle = Mathf.Atan2(dir.y - lastDir.y, dir.x - lastDir.x) * Mathf.Rad2Deg;                                   //oprócz przedpierwszego segmentu bo on zmienia się w zależności od ruchu głowy
            snakeBody[1].GetComponent<SpriteRenderer>().sprite = turn;                                                    //po całym ruchu zaaktualizuj ostatni cel podróży 
            snakeBody[1].transform.eulerAngles = new Vector3(0, 0, angle - 225);
        }
        else
        {
            snakeBody[1].GetComponent<SpriteRenderer>().sprite = body;
            snakeBody[1].transform.eulerAngles = snakeBody[0].transform.eulerAngles;
        }
        lastDir = dir;
        int destination = segmentsPlacement[0] + getValueFromDirection(dir);
        if (destination < 101 || destination > 816 || destination % 100 > 16 || destination % 100 == 0)
        {
            Debug.Log("koniec");
            instance.gameEnd();
            return;
        }
        segmentsPlacement[0] += getValueFromDirection(dir);
        snakeBody[0].transform.position += new Vector3(dir.x, dir.y, 0);
        if (segmentsPlacement[0] == instance.applePlacement)
        {
            grow();
            instance.DestroyApple();
            instance.CreateApple();
        }
        int playerHead = segmentsPlacement[0];
        int playerRow = playerHead / 100;
        int playerCol = playerHead % 100;
        if (instance.map[playerRow - 1, playerCol - 1].isSnake || !instance.map[playerRow - 1, playerCol - 1].isRoad || instance.map[playerRow-1,playerCol-1].isTail)
        {
            instance.gameEnd();
            return;
        }
        else
        {
            instance.map[playerRow - 1, playerCol - 1].isSnake = true;
        }
        instance.moveSecondSnake();
    }

    
    public void grow()
    {
        var i = Instantiate(segment, snakeBody[snakeBody.Count - 1].transform.position, Quaternion.identity);
        snakeBody.Add(i);
        i.GetComponent<SpriteRenderer>().sprite = tail;
        i.transform.rotation = snakeBody[snakeBody.Count - 2].transform.rotation;
        segmentsPlacement.Add(segmentsPlacement[segmentsPlacement.Count - 1]);
        if (snakeBody.Count > 2)
            snakeBody[snakeBody.Count - 2].GetComponent<SpriteRenderer>().sprite = body;
    }

    public int getValueFromDirection(Vector2 direction)
    {
        if (direction.x > 0)
        {
            return 1;
        }
        if (direction.x < 0)
        {
            return -1;
        }
        if (direction.y > 0)
        {
            return -100;
        }
        if (direction.y < 0)
        {
            return 100;
        }
        return 0;
    }
}
