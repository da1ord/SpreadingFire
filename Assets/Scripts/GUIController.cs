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
    GameObject objects_;
    public List<GameObject> plants_;
    bool isSimulating_;

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
        objects_ = GameObject.Find( "Vegetation" );
        plants_ = new List<GameObject>();
        isSimulating_ = true;

        selectedMode_ = Mode.AddPlant;
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
        onBtnClearClick();

    }

    // Clears all the existing plants
    void onBtnClearClick()
    {
        foreach( Transform plant in objects_.transform )
        {
            Destroy( plant.gameObject );
        }
    }

    // Plays / stops the simulation
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

        isSimulating_ = !isSimulating_;

        if( isSimulating_ )
        {
            Time.timeScale = 1;
        }
        else
        {
            Time.timeScale = 0;
        }
    }

    // Lights up several randomly selected plants
    void onBtnFireClick()
    {
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
        windDirectionArrow_.transform.localScale = new Vector3( slWindSpeed.value / 20.0f, 1.0f, slWindSpeed.value / 10.0f );
    }

    // Changes the wind direction. Affects the direction of the fire spreading
    void onWindDirectionSliderChanged()
    {
        windDirectionArrow_.transform.rotation = Quaternion.AngleAxis( slWindDirection.value, Vector3.up );
    }

    public Mode GetMode()
    {
        return selectedMode_;
    }
}
