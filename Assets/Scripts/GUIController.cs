using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIController : MonoBehaviour
{
    // GUI controls
    public Button btnGenerate;
    public Button btnClear;
    public Button btnSimulate;
    public Button btnFire;
    public Button btnMode;
    public Button btnQuit;
    public Text lbMode;
    public Slider slWindSpeed;
    public Slider slWindDirection;

    // Wind direction arrow showing the wind direction
    GameObject windDirectionArrow_;
    // WInd direction vector
    Vector3 windDirection_;
    // Plants list
    public List<GameObject> plants_;
    // Flag indicating if the simulation is running
    bool isSimulating_;
    // Color block for simulating button background change
    ColorBlock cb;

    // Terrain object
    public Terrain terrain_;
    // Plant object to instantiate
    public GameObject bush_;

    // Enmuerator for mode selection
    public enum Mode
    {
        AddPlant,
        RemovePlant,
        ToggleFire
    };
    // Selected mode variable
    Mode selectedMode_;

    // Initialization
    void Start()
    {
        // Initialize listeners for GUI controls
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

        // Get wind arrow gameobject
        windDirectionArrow_ = GameObject.Find( "WindDirectionArrow" );
        // Set wind direction vector
        windDirection_ = new Vector3( 0.0f, 0.0f, 1.0f );
        // Call wind sliders routines (in case the wind is set before runtime)
        onWindSpeedSliderChanged();
        onWindDirectionSliderChanged();
        
        // Initialize plants list
        plants_ = new List<GameObject>();

        // Stop simulation
        isSimulating_ = false;
        Time.timeScale = 0;
        // Set simulate button color
        cb = btnSimulate.colors;
        cb.normalColor = Color.red;
        cb.highlightedColor = Color.red;
        btnSimulate.colors = cb;

        // Set selected mode
        selectedMode_ = Mode.AddPlant;
        // Set selected mode text
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
            // Fill neighbours
            foreach( GameObject p in plants_ )
            {
                if( ( plant.transform.position - p.transform.position ).magnitude < 5f )
                {
                    plant.GetComponent<BushHealth>().neighbourPlants_.Add( p );
                    p.GetComponent<BushHealth>().neighbourPlants_.Add( plant );
                }
            }
            // Add the plant to the list of plants
            plants_.Add( plant );

            // Check if wind speed is greater than 0
            if( slWindSpeed.value > 0 )
            {
                // Fill neighbouring plants in the wind direction
                FillWindNeighbours();
            }
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
        // Change simulation state variable
        isSimulating_ = !isSimulating_;

        // Start/resume the simulation
        if( isSimulating_ )
        {
            // Set simulation button color and start the simulation
            cb.normalColor = Color.green;
            cb.highlightedColor = Color.green;
            btnSimulate.colors = cb;

            Time.timeScale = 1;
        }
        // Stop the simulation
        else
        {
            // Set simulation button color and stop the simulation
            cb.normalColor = Color.red;
            cb.highlightedColor = Color.red;
            btnSimulate.colors = cb;

            Time.timeScale = 0;
        }
    }

    // Lights up several randomly selected plants
    void onBtnFireClick()
    {
        // Return if there are no plants in the scene
        if( plants_.Count == 0 )
        {
            return;
        }

        // Percentage of plants to light up
        int burningPercentage = Random.Range( 20, 50 );
        burningPercentage *= plants_.Count;
        burningPercentage /=  100;

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
            plants_[indices[i]].GetComponent<BushHealth>().StartsBurning();
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
        // Scale the wind direction arrow object
        windDirectionArrow_.transform.localScale = new Vector3( 
            slWindSpeed.value / 20.0f, 
            1.0f, 
            slWindSpeed.value / 10.0f );

        // Rotate the wind direction vector
        //windDirection_ = Quaternion.Euler( 0.0f, slWindDirection.value, 0.0f ) * Vector3.forward;

        // Check if wind speed is greater than 0
        if( slWindSpeed.value > 0 )
        {
            // Fill neighbouring plants in the wind direction
            FillWindNeighbours();
        }
    }

    // Changes the wind direction. Affects the direction of the fire spreading
    void onWindDirectionSliderChanged()
    {
        // Rotate the wind direction arrow object
        windDirectionArrow_.transform.rotation = 
            Quaternion.AngleAxis( slWindDirection.value, Vector3.up );

        // Rotate the wind direction vector
        windDirection_ = Quaternion.Euler( 0.0f, slWindDirection.value, 0.0f ) * Vector3.forward;

        // Check if wind speed is greater than 0
        if( slWindSpeed.value > 0 )
        {
            // Fill neighbouring plants in the wind direction
            FillWindNeighbours();
        }
    }

    // Mode getter
    public Mode GetMode()
    {
        return selectedMode_;
    }

    // Fill neighboring plants in the wind direction
    public void FillWindNeighbours()
    {
        // Go through all plants in scene
        foreach( GameObject plant in plants_ )
        {
            // Clear wind neighbours
            plant.GetComponent<BushHealth>().windNeighbourPlants_.Clear();

            // Go through all plants in scene
            foreach( GameObject plant2 in plants_ )
            {
                // Check if the two plants are not the same one
                if( plant2 != plant )
                {
                    // Get distance between plants
                    Vector3 v1 = plant2.transform.position - plant.transform.position;
                    // Check if plants are within windspeed distance
                    if( v1.magnitude < slWindSpeed.value )
                    {
                        // Get angle between to-plant drection and wind
                        float angleToPlant = Vector3.Angle( windDirection_, v1 );
                        // Check if angle has proper value (within 15 degrees)
                        if( angleToPlant < 15.0f )
                        {
                            RaycastHit hit;
                            // Cast ray from one plant to the other one
                            Ray ray = new Ray( plant.transform.position, v1 );
                            // Check if no terrain in the way
                            if( !Physics.Raycast( ray, out hit, v1.magnitude, LayerMask.GetMask( "Terrain" ) ) )
                            {
                                // Add wind neighbour
                                plant.GetComponent<BushHealth>().windNeighbourPlants_.Add( plant2 );
                            }
                        }
                    }
                }
            }
        }
    }

    // Check if plant's neighbours are burning. If some of them is burning, 
    //  set this plant on fire
    public void IsNearFire( GameObject plant )
    {
        // Neighbours test
        foreach( GameObject p in plant.GetComponent<BushHealth>().neighbourPlants_ )
        {
            if( p.GetComponent<BushHealth>().IsBurning() )
            {
                plant.GetComponent<BushHealth>().SetNearFire();
                return;
            }
        }
        // Wind neighbours test
        foreach( GameObject p in plants_ )
        {
            foreach( GameObject p2 in p.GetComponent<BushHealth>().windNeighbourPlants_ )
            {
                if( p2 == plant && p.GetComponent<BushHealth>().IsBurning() )
                {
                    plant.GetComponent<BushHealth>().SetNearFire();
                    return;
                }
            }
        }
    }
}
