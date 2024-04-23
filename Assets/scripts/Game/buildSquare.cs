using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buildSquare : MonoBehaviour
{
    // Start is called before the first frame update
    public gameManagement GM;
    public string id;
    void Start()
    {
        GM=GameObject.Find("GameManager").GetComponent<gameManagement>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnMouseDown() {
        Instantiate(GM.mainGame.emptySquare,transform.position,Quaternion.identity);
        Destroy(gameObject);
    }
}
