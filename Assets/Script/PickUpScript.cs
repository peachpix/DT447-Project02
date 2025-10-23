using UnityEngine;
using TMPro; // optional if you want to show count on screen

public class PickUpScript : MonoBehaviour
{
    public Camera fpsCam;
    public float pickUpRange = 5f;
    public KeyCode grabKey = KeyCode.E;
    public int collectedCount = 0; // count collected pickups

    public TMP_Text counterText; // optional UI to show count

    void Update()
    {
        if (Input.GetKeyDown(grabKey))
        {
            RaycastHit hit;
            if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, pickUpRange))
            {
                if (hit.transform.CompareTag("Pickup"))
                {
                    CollectObject(hit.transform.gameObject);
                }
            }
        }
    }

    void CollectObject(GameObject obj)
    {
        collectedCount++;
        Debug.Log("Collected: " + collectedCount);
        if (counterText)
            counterText.text = "Collected: " + collectedCount;

        // Hide the object (simulate pickup)
        obj.SetActive(false);
    }
}
