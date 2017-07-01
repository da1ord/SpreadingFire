using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIController : MonoBehaviour
{
    public Button btnGenerate;
    public Button btnClear;
    public Button btnSimulate;
    public Button btnFire;
    public Button btnMode;
    public Button btnQuit;
    public Text lbMode;
    public Slider slWindSpeed;
    public Slider slWindDirection;

    GameObject windDirectionArrow_;
    Vector3 windDirection_;
    GameObject objects_;
    public List<GameObject> plants_;
    bool isSimulating_;
    ColorBlock cb;

    public Terrain terrain_;
    public GameObject bush_;

    public enum Mode
    {
        AddPlant,
        RemovePlant,
        ToggleFire
    };

    Mode selectedMode_;

    // Initialization
    void Start()
    {
        btnGenerate.onClick.AddListener( onBtnGenerateClick );
        btnClear.onClick.AddListener( onBtnClearClick );
        btnSimulate.onClick.AddListener( onBtnSimulateClick );
        btnFire.onClick.AddListener( onBtnFireClick );
        btnMode.onClick.AddListener( onBtnModeClick );
        btnQuit.onClick.AddListener( onBtnQuitClick );

        slWindSpeed.onValueChanged.AddListener( delegate {
            onWindSpeedSliderChanged();
        } );

        slWindDirection.onValueChanged.AddListener( delegate {
            onWindDirectionSliderChanged();
        } );

        windDirectionArrow_ = GameObject.Find( "WindDirectionArrow" );
        windDirection_ = new Vector3( 0.0f, 0.0f, 1.0f );
        onWindSpeedSliderChanged();
        onWindDirectionSliderChanged();

        objects_ = GameObject.Find( "Vegetation" );
        plants_ = new List<GameObject>();

        isSimulating_ = false;
        Time.timeScale = 0;
        cb = btnSimulate.colors;
        cb.normalColor = Color.red;
        cb.highlightedColor = Color.red;
        btnSimulate.colors = cb;

        selectedMode_ = Mode.ToggleFire;
        lbMode.text = selectedMode_.ToString();
    }
	
	// Update call
	void Update()
    {
		
	}

    /***************************************************************************
     * GUI controls handling
     **************************************************************************/
    // Generates new random plants on the terrain 
    void onBtnGenerateClick()
    {
        // Clear all plants
        onBtnClearClick();

        // Generate 100 plants
        for( int i = 0; i < 100; i++ )
        {
            // Generate random plant position
            Vector3 plantPosition = new Vector3( 
                Random.Range( 5.0f, 123.0f ),
                0.0f, 
                Random.Range( 5.0f, 123.0f ) );
            // Sample terrain height
            plantPosition.y = terrain_.SampleHeight( plantPosition );

            // Initialize flag indicating plant's position
            bool plantOk = false;
            // Repeat until plant position is OK
            while( !plantOk )
            {
                // Initialize flag indicating if the plant is too close to other 
                //  plants
                bool tooClose = false;
                // Go through all plants on terrain
                foreach( GameObject p in plants_ )
                {
                    // Check if new plant is too close to other one
                    if( ( plantPosition - p.transform.position ).magnitude < 2 )
                    {
                        // Plant is too close. Generate new position and go 
                        //  through all plants again
                        plantPosition = new Vector3( 
                            Random.Range( 5.0f, 123.0f ), 
                            0.0f, 
                            Random.Range( 5.0f, 123.0f ) );
                        plantPosition.y = terrain_.SampleHeight( plantPosition );
                        // Set flag indicating that the plant was too close
                        tooClose = true;
                        break;
                    }
                }
                // If the plant was not too close to others, set as OK
                if( !tooClose )
                {
                    plantOk = true;
                }
            }

            // Instantiate the plant
            GameObject plant = Instantiate( bush_, plantPosition,
                Quaternion.identity, GameObject.Find( "Vegetation" ).transform );
            /* Fill neighbours */
            foreach( GameObject p in plants_ )
            {
                if( ( plant.transform.position - p.transform.position ).magnitude < 5f )
                {
                    plant.GetComponent<BushHealth>().neighbourPlants_.Add( p );
                    p.GetComponent<BushHealth>().neighbourPlants_.Add( plant );
                }
            }
            /**/
            // Add the plant to the list of plants
            plants_.Add( plant );
        }
    }

    // Clears all the existing plants
    void onBtnClearClick()
    {
        foreach( GameObject p in plants_ )
        {
            Destroy( p );
        }
        plants_.Clear();
    }

    // Stops/resumes the simulation
    void onBtnSimulateClick()
    {
        /* TODO: remove */
        foreach( GameObject plant in plants_ )
        {
            foreach( GameObject plant2 in plants_ )
            {
                if( plant2 != plant )
                {
                    if( (plant.transform.position - plant2.transform.position).magnitude < 5 )
                    {
                        if( plant.GetComponent<BushHealth>().IsBurning() && !plant2.GetComponent<BushHealth>().IsBurning() )
                        {
                            plant2.GetComponent<BushHealth>().SetNearFire();
                        }
                    }
                }
            }
        }
        /**/

        // Change simulation state variable
        isSimulating_ = !isSimulating_;

        // Start/resume simulation
        if( isSimulating_ )
        {
            cb.normalColor = Color.green;
            cb.highlightedColor = Color.green;
            btnSimulate.colors = cb;

            Time.timeScale = 1;
        }
        // Stop simulation
        else
        {
            cb.normalColor = Color.red;
            cb.highlightedColor = Color.red;
            btnSimulate.colors = cb;

            Time.timeScale = 0;
        }
    }

    // Lights up several randomly selected plants
    void onBtnFireClick()
    {
        /**/
        foreach( GameObject plant in plants_ )
        {
            if( plant.GetComponent<BushHealth>().IsBurning() )
            {
                foreach( GameObject plant2 in plant.GetComponent<BushHealth>().windNeighbourPlants_ )
                {
                    plant2.GetComponent<BushHealth>().HasBurnt();// StartedBurning();
                }
            }
        }
        /**/
        return;
        // Percentage of plants to light up
        int burningPercentage = Random.Range( 30, 70 );
        // List of unique plant indices
        List<int> indices = new List<int>( burningPercentage );
        // Generate random unique indices
        for( int i = 0; i < burningPercentage; i++ )
        {
            // Generate index number
            int index = Random.Range( 0, plants_.Count );
            // Check for uniqueness of indices
            while( indices.Contains( index ) )
            {
                // Generate new index till it is unique
                index = Random.Range( 0, plants_.Count );
            }
            // Add generated index into list of indices
            indices.Add( index );
        }

        // Light up the selected plants
        for( int i = 0; i < indices.Count; i++ )
        {
            plants_[indices[i]].GetComponent<BushHealth>().StartedBurning();
        }
    }

    // Toggles between various mouse interaction modes: Add, Remove, Toggle Fire
    void onBtnModeClick()
    {
        selectedMode_ = (Mode)((int)++selectedMode_ % 3);
        lbMode.text = selectedMode_.ToString();
    }

    // Quits the application
    void onBtnQuitClick()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // Changes the wind speed. Affects the speed of the fire spreading
    void onWindSpeedSliderChanged()
    {
        windDirectionArrow_.transform.localScale = new Vector3( 
            slWindSpeed.value / 20.0f, 
            1.0f, 
            slWindSpeed.value / 10.0f );

        windDirection_ = Quaternion.Euler( 0.0f, slWindDirection.value, 0.0f ) * Vector3.forward;// * slWindSpeed.value / 2.0f;
        /* if speed > 0 */
        //return;
        foreach( GameObject plant in plants_ )
        {
            plant.GetComponent<BushHealth>().windNeighbourPlants_.Clear();
            //if( plant.GetComponent<BushHealth>().IsBurning())
            foreach( GameObject plant2 in plants_ )
            {
                if( plant2 != plant )
                //if( plant2 != plant && plant.GetComponent<BushHealth>().IsBurning() )
                {
                    Vector3 v1 = plant2.transform.position - plant.transform.position;
                    // Check if the plant is within windspeed distance
                    if( v1.magnitude < slWindSpeed.value )
                    {
                        Vector3 v2 = windDirection_;

                        float angleToPlant = Vector3.Angle( v2, v1 );//plant.GetComponent<BushHealth>().GetAngleToObject( plant2.transform.position, windDirection_ );//
                        // Check if plant is in wind direction
                        if( angleToPlant < 15.0f )
                        {
                            RaycastHit hit;
                            Ray ray = new Ray( plant.transform.position, v1 );
                            // Check if no terrain in the way
                            //if( Physics.Raycast( ray, out hit, v1.magnitude ) )
                            if( !Physics.Raycast( ray, out hit, v1.magnitude, LayerMask.GetMask( "Terrain" ) ) )
                            {
                                //if( hit.transform.gameObject.name != "Terrain" )
                                {
                                    //plant2.GetComponent<BushHealth>().StartedBurning();
                                    plant.GetComponent<BushHealth>().windNeighbourPlants_.Add( plant2 );
                                }
                            }
                            else
                            {
                                plant.GetComponent<BushHealth>().HasBurnt();
                                plant2.GetComponent<BushHealth>().HasBurnt();
                            }
                        }
                    }
                }
            }
        }
        #region backup
        //foreach( GameObject plant in plants_ )
        //{
        //    if( plant.GetComponent<BushHealth>().IsBurning() )
        //    {
        //        foreach( GameObject plant2 in plants_ )
        //        {
        //            if( plant2 != plant )
        //            {
        //                Vector3 v1 = plant2.transform.position - plant.transform.position;
        //                Vector3 v2 = windDirection_;

        //                float angleToPlant = Vector3.Angle( v2, v1 );//plant.GetComponent<BushHealth>().GetAngleToObject( plant2.transform.position, windDirection_ );//
        //                // Check if plant is in wind direction
        //                if( angleToPlant > -15.0f && angleToPlant < 15.0f )
        //                {
        //                    //RaycastHit hit;
        //                    //Ray ray = new Ray( plant.transform.position, windDirection_ );
        //                    //if( Physics.Raycast( ray, out hit, slWindSpeed.value / 2.0f, LayerMask.GetMask( "Vegetation" ) ) )
        //                    //{
        //                    //    //GameObject plant = hit.transform.gameObject;
        //                    //}
        //                    plant2.GetComponent<BushHealth>().StartedBurning();
        //                }
        //            }
        //        }
        //    }
        //}
        #endregion
    }

    // Changes the wind direction. Affects the direction of the fire spreading
    void onWindDirectionSliderChanged()
    {
        windDirectionArrow_.transform.rotation = 
            Quaternion.AngleAxis( slWindDirection.value, Vector3.up );
        windDirection_ = Quaternion.Euler( 0.0f, slWindDirection.value, 0.0f ) * Vector3.forward;// * slWindSpeed.value / 2.0f;
        //Debug.DrawLine( new Vector3( 0, 0, 0 ), 128 * windDirection_, Color.red );
        /* if speed > 0 */
    }

    public Mode GetMode()
    {
        return selectedMode_;
    }
}
