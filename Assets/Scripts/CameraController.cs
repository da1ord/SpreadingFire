using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    GUIController GUIController_;
    RaycastHit hit;
    Ray ray;

    public GameObject bush_;

	// Initialization
	void Start()
    {
        GUIController_ = GameObject.Find( "GUI" ).GetComponent<GUIController>();
	}
	
	// Update call
	void Update()
    {
        // LMB pressed and not clicking GUI
        if( Input.GetMouseButtonDown( 0 ) && !EventSystem.current.IsPointerOverGameObject() )
        {
            ray = Camera.main.ScreenPointToRay( Input.mousePosition );

            if( Physics.Raycast( ray, out hit, 1000.0f, LayerMask.GetMask( "Vegetation" ) ) )
            {
                GameObject plant = hit.transform.gameObject;
                if( GUIController_.GetMode() == GUIController.Mode.RemovePlant )
                {
                    Destroy( plant );
                }
                else if( GUIController_.GetMode() == GUIController.Mode.ToggleFire )
                {
                    if( plant.GetComponent<BushHealth>().IsBurning() )
                    {
                        // extinguish
                        plant.GetComponent<BushHealth>().Extinguish();
                    }
                    else
                    {
                        // set on fire
                        plant.GetComponent<BushHealth>().StartedBurning();
                    }
                }

                //if( hit.transform.gameObject.GetComponent<BushHealth>().IsBurning() )
                //{
                //    hit.transform.gameObject.GetComponent<BushHealth>().HasBurnt();
                //}
                //else
                //{
                //    hit.transform.gameObject.GetComponent<BushHealth>().StartedBurning();
                //}
            }
            else if( Physics.Raycast( ray, out hit, 1000.0f, LayerMask.GetMask( "Terrain" ) ) )
            {
                // create plant
                if( GUIController_.GetMode() == GUIController.Mode.AddPlant )
                {
                    GameObject plant = Instantiate( bush_, hit.point, 
                        Quaternion.identity, GameObject.Find( "Vegetation" ).transform );
                    GUIController_.plants_.Add( plant );
                }
            }
        }
	}
}
