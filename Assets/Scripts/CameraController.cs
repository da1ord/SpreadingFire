using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    // GUIController object
    GUIController GUIController_;
    // Hit variable for raycasting
    RaycastHit hit;
    // Ray variable for raycasting
    Ray ray;
    
    // Mouse around x-axis rotation
    float xRot_ = -65.0f;
    // Mouse sensitivity constant
    const float mouseSensitivity_ = 3.0f;
    // Instantiable plant object
    public GameObject bush_;

	// Initialization
	void Start()
    {
        // Get GUIController object
        GUIController_ = GameObject.Find( "GUI" ).GetComponent<GUIController>();
	}
	
	// Update call
	void Update()
    {
        // RMB for camera rotating
        if( Input.GetMouseButton( 1 ) )
        {
            float yRot_ = transform.localEulerAngles.y + Input.GetAxis( "Mouse X" ) * mouseSensitivity_;
            xRot_ += Input.GetAxis( "Mouse Y" ) * mouseSensitivity_;
            xRot_ = Mathf.Clamp( xRot_, -90f, 90f );

            transform.localEulerAngles = new Vector3( -xRot_, yRot_, 0 );
        }

        // WASD for camera movement 
        if( Input.GetKey( KeyCode.W ) )
        {
            transform.position += transform.forward;
        }
        if( Input.GetKey( KeyCode.S ) )
        {
            transform.position -= transform.forward;
        }
        if( Input.GetKey( KeyCode.A ) )
        {
            transform.position -= transform.right;
        }
        if( Input.GetKey( KeyCode.D ) )
        {
            transform.position += transform.right;
        }

        // LMB pressed and not clicking GUI
        if( Input.GetMouseButtonDown( 0 ) && !EventSystem.current.IsPointerOverGameObject() )
        {
            // Create ray from camera
            ray = Camera.main.ScreenPointToRay( Input.mousePosition );

            // Test for vegetation hit
            if( Physics.Raycast( ray, out hit, 1000.0f, LayerMask.GetMask( "Vegetation" ) ) )
            {
                GameObject plant = hit.transform.gameObject;
                // Plant removing
                if( GUIController_.GetMode() == GUIController.Mode.RemovePlant )
                {
                    // Remove from all neighbours and wind neighbours
                    foreach( GameObject p in GUIController_.plants_ )
                    {
                        p.GetComponent<BushHealth>().RemoveFromNeighbours( plant );
                    }
                    // Remove from list of plants
                    GUIController_.plants_.Remove( plant );
                    // Destroy object
                    Destroy( plant );
                }
                // Lighting up/extinguishing the plant
                else if( GUIController_.GetMode() == GUIController.Mode.ToggleFire )
                {
                    // Check if plant is not burning or already burnt - extinguish it
                    if( plant.GetComponent<BushHealth>().IsBurning() ||
                        plant.GetComponent<BushHealth>().IsBurnt() )
                    {
                        // Extinguish plant
                        plant.GetComponent<BushHealth>().Extinguish();
                        // Check if plant is near fire
                        GUIController_.IsNearFire( plant );
                        // Check if plant's neighbours are still near fire
                        foreach( GameObject p in plant.GetComponent<BushHealth>().neighbourPlants_ )
                        {
                            if( p.GetComponent<BushHealth>().IsNearFire() )
                            {
                                p.GetComponent<BushHealth>().ClearNearFire();
                                GUIController_.IsNearFire( p );
                            }
                        }
                        // Check if plant's wind neighbours are still near fire
                        foreach( GameObject p in plant.GetComponent<BushHealth>().windNeighbourPlants_ )
                        {
                            if( p.GetComponent<BushHealth>().IsNearFire() )
                            {
                                p.GetComponent<BushHealth>().ClearNearFire();
                                GUIController_.IsNearFire( p );
                            }
                        }
                    }
                    else
                    {
                        // Light up plant
                        plant.GetComponent<BushHealth>().StartsBurning();
                    }
                }
            }
            // Test for terrain hit
            else if( Physics.Raycast( ray, out hit, 1000.0f, LayerMask.GetMask( "Terrain" ) ) )
            {
                // Plant adding
                if( GUIController_.GetMode() == GUIController.Mode.AddPlant )
                {
                    // Instantiate the plant
                    GameObject plant = Instantiate( bush_, hit.point, 
                        Quaternion.identity, GameObject.Find( "Vegetation" ).transform );
                    // Add plant to the plants list
                    GUIController_.plants_.Add( plant );

                    // Fill neighbours
                    foreach( GameObject p in GUIController_.plants_ )
                    {
                        // Fill neighbours if the distance is less than 5
                        if( ( plant.transform.position - p.transform.position ).magnitude < 5f )
                        {
                            plant.GetComponent<BushHealth>().neighbourPlants_.Add( p );
                            p.GetComponent<BushHealth>().neighbourPlants_.Add( plant );
                        }
                    }
                    // Fill neighbouring plants in the wind direction
                    // Check if wind speed is greater than 0
                    if( GUIController_.slWindSpeed.value > 0 )
                    {
                        GUIController_.FillWindNeighbours();
                    }
                }
            }
        }
	}
}
