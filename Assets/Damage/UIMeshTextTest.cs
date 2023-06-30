using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class UIMeshTextTest : MonoBehaviour
    {
        UIMeshText mesh;
        public Font font;
        //public Text text;
        //private List<int> m_meshArry = new List<int>();
        private Font instance;

        private int loop = 10;
        //int i = 0; 
        //private string str = "1233232323232";

        private List<UIMeshText> _meshes = new List<UIMeshText>();
        
        private static List<Vector3> randomPos = new List<Vector3>();

        public Transform damageLayer;
        public DamageNumManager manager;
        
        void Start()
        {
            manager.Initialize();
            for (int j = 0; j < loop; j++)
            {
                randomPos.Add(new Vector3(300, 100+j * 60, 0));
            }    
                
            mesh = new UIMeshText(damageLayer.transform, "mymesh", font, 100000, 1500, Vector3.one * 0.1f);
            
            _meshes?.Add(mesh);

            instance = Instantiate(font);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Plane p = new Plane();
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                p.Raycast(ray, out var enter);
                var worldPos = ray.GetPoint(enter);

                manager.ShowDamage(5555, worldPos);
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                for (int j = 0; j < 200; j++)
                {
                    var screenPos = new Vector3(Random.Range(0, Screen.width), Random.Range(0, Screen.height), 0);
                    var worldPos = Camera.main.ScreenToWorldPoint(screenPos);
                    manager.ShowDamageParam("1234", worldPos);
                }
            }
        }
    }


}