using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BushHealth : MonoBehaviour
{
    // Plant renderer object for color changing
    Renderer renderer_;
    // GUIController object
    GUIController GUIController_;

    // Health
    float health_;
    // Flag if plant is near the fire
    bool isNearFire_;
    // Burning flag
    bool isBurning_;
    // Burnt flag
    bool isBurnt_;
    
    // List of neighbour plants
    public List<GameObject> neighbourPlants_ = new List<GameObject>();
    // List of wind neighbour plants
    public List<GameObject> windNeighbourPlants_ = new List<GameObject>();

    // Initialization
    void Start()
    {
        // Set starting health
        health_ = 5f;
        // Plant is not near the fire
        isNearFire_ = false;
        // Plant is not burning at the beginning
        isBurning_ = false;
        // Plant is 'alive'
        isBurnt_ = false;

        // Get GUIController component
        GUIController_ = GameObject.Find( "GUI" ).GetComponent<GUIController>();
        // Get renderer component
        renderer_ = GetComponent<Renderer>();
        // Set starting green color
        renderer_.material.color = Color.green;
    }

    // Update call
    void Update()
    {
        // Check if the plant is 'alive'
        if( !isBurnt_ )
        {
            // Check if the plant is burning
            if( isBurning_ || isNearFire_ )
            {
                // Decrease its health. Burning plant's health decrease faster
                if( isBurning_ )
                {
                    health_ -= 1.5f * Time.deltaTime;
                }
                else
                {
                    health_ -= Time.deltaTime;
                    // Set orangish (near fire) color
                    Color c = new Color( 0.9f, 0.7f, 0 );
                    renderer_.material.color = c;
                }
            }

            // Check if the health is below/equal to burning limit and not yet 
            //  burning. If so, it starts burning
            if( health_ <= 3.0f && !isBurning_ )
            {
                StartsBurning();
            }

            // Check if the health is below/equal to zero
            if( health_ <= 0.0f )
            {
                // The plant has burnt
                HasBurnt();
            }
        }
    }

    // Plant starts to burn
    public void StartsBurning()
    {
        // Set is burning flag
        isBurning_ = true;
        // Set proper health of plant that just started to burn
        health_ = 3f;
        // Set red color
        renderer_.material.color = Color.red;

        // Go through neighbours and set them near fire if they are not burning,
        //  near fire or already burnt
        foreach( GameObject p in neighbourPlants_ )
        {
            if( !p.GetComponent<BushHealth>().IsBurning() && 
                !p.GetComponent<BushHealth>().IsNearFire() && 
                !p.GetComponent<BushHealth>().IsBurnt() )
            {
                p.GetComponent<BushHealth>().SetNearFire();
            }
        }
        // Go through wind neighbours and set them near fire if they are not 
        // burning, near fire or already burnt
        foreach( GameObject p in windNeighbourPlants_ )
        {
            if( !p.GetComponent<BushHealth>().IsBurning() && 
                !p.GetComponent<BushHealth>().IsNearFire() &&
                !p.GetComponent<BushHealth>().IsBurnt() )
            {
                p.GetComponent<BushHealth>().SetNearFire();
            }
        }
    }

    // Plant is near fire
    public void SetNearFire()
    {
        isNearFire_ = true;
    }

    // Plant is not near fire
    public void ClearNearFire()
    {
        isNearFire_ = false;
    }

    // Plant has burnt
    public void HasBurnt()
    {
        // Set burnt flag
        isBurnt_ = true;
        // Clear burning flag
        isBurning_ = false;
        // Set black color
        renderer_.material.color = Color.black;

        // Plant has burnt. Check if neighbours are affected by other burning 
        //  plants
        // Go through neighbours
        //  If the neighbour plant is not burning, check if it is near fire 
        //  of other not burnt plant
        foreach( GameObject p in neighbourPlants_ )
        {
            if( p.GetComponent<BushHealth>().isNearFire_ )
            {
                p.GetComponent<BushHealth>().ClearNearFire();
                GUIController_.IsNearFire( p );
            }
        }
        // Go through wind neighbours
        //  If the neighbour plant is not burning, check if it is near fire 
        //  of other not burnt plant
        foreach( GameObject p in windNeighbourPlants_ )
        {
            if( p.GetComponent<BushHealth>().isNearFire_ )
            {
                p.GetComponent<BushHealth>().ClearNearFire();
                GUIController_.IsNearFire( p );
            }
        }
    }

    // Extinguish plant
    public void Extinguish()
    {
        health_ = 5f;
        isBurnt_ = false;
        isBurning_ = false;
        isNearFire_ = false;
        renderer_.material.color = Color.green;
    }

    // Remove deleted plant from neighbours lists
    public void RemoveFromNeighbours( GameObject plant )
    {
        neighbourPlants_.Remove( plant );
        windNeighbourPlants_.Remove( plant );
    }

    // Flag indicating if plant is burning
    public bool IsBurning()
    {
        return isBurning_;
    }
    // Flag indicating if plant is near fire
    public bool IsNearFire()
    {
        return isNearFire_;
    }
    // Flag indicating if plant has burnt
    public bool IsBurnt()
    {
        return isBurnt_;
    }
    // Plant health getter
    public float GetHealth()
    {
        return health_;
    }
}
