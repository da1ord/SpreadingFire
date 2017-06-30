using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BushHealth : MonoBehaviour
{
    Renderer renderer_;
    
    // Health
    float health_;
    // Flag if bush is near the fire
    bool isNearFire_;
    // Burning flag
    bool isBurning_;
    // Burnt flag
    bool isBurnt_;

    float flameDistance_;
    Color defaultColor_;

    // Initialization
    void Start()
    {
        // Set starting health
        health_ = 5f;
        // Bush is not near the fire
        isNearFire_ = false;
        // Bush is not burning at the beginning
        isBurning_ = false;
        // Bush is 'alive'
        isBurnt_ = false;

        flameDistance_ = 5f;

        // Get renderer component
        renderer_ = GetComponent<Renderer>();

        defaultColor_ = renderer_.material.color;
    }

    // Update call
    void Update()
    {
        // Check if the bush is 'alive'
        if( !isBurnt_ )
        {
            // Check if the bush is burning
            if( isBurning_ || isNearFire_ )
            {
                // TODO: Burning bush's health should decrease faster
                // Decrease its health
                health_ -= Time.deltaTime;
                Debug.Log( health_ );
            }

            // Check if the health is below/equal to burning limit and not yet 
            //  burning
            if( health_ <= 3.0f && !isBurning_ )
            {
                StartedBurning();
            }

            // Check if the health is below/equal to zero
            if( health_ <= 0.0f )
            {
                // The bush has burnt
                isBurnt_ = true;
                HasBurnt();
            }
        }
    }

    public void SetNearFire()
    {
        isNearFire_ = true;
    }

    public void ClearNearFire()
    {
        isNearFire_ = false;
    }

    public void StartedBurning()
    {
        isBurning_ = true;
        // Set red color
        renderer_.material.color = new Color( 255f, 0f, 0f, 1f );
    }

    public void HasBurnt()
    {
        // Set black color
        renderer_.material.color = new Color( 0f, 0f, 0f, 1f );
    }

    public void Extinguish()
    {
        health_ = 5f;
        isBurnt_ = false;
        isBurning_ = false;
        renderer_.material.color = defaultColor_;
    }

    public bool IsBurning()
    {
        return isBurning_;
    }
}
