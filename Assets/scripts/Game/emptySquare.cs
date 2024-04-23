using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class emptySquare : MonoBehaviour
{
    public gameManagement GM;
    public int id;
    public bool isBuild;
    int row;
    int col;
    // Start is called before the first frame update
    void Start()
    {
        GM=GameObject.Find("GameManager").GetComponent<gameManagement>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnMouseDown() {
        makeSquare();
    }

    public void makeSquare()
    {
        int row=id/100;
        int col=(id-(id/100)*100);
        if(!isBuild)
        {
            GetComponent<SpriteRenderer>().sprite=GM.buildSquare;
            isBuild=true;
            GM.mainGame.map[row-1,col-1].isRoad=true;
        }else
        {
            GetComponent<SpriteRenderer>().sprite=null;
            isBuild=false;
            GM.mainGame.map[row-1,col-1].isRoad=false;
        }
    }
}
