using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camp : MonoBehaviour
{
    Vector2 nearestRiver;
    Vector2 nearestMarsh;
    Vector2 nearestMoose;
    int food;
    public GameObject human;
    GameObject campHuman;
    Vector2 destination;
    public float humanSpeed;
    [SerializeField]
    Material mooseMaterial;
    [SerializeField]
    Material fishMaterial;
    [SerializeField]
    Material marshMaterial;
    [SerializeField]
    int startFood;
    [SerializeField]
    int foodPerTrip;

    // Start is called before the first frame update
    void Start()
    {
        campHuman = Instantiate(human, this.transform.position, Quaternion.identity);
        campHuman.SetActive(false);
        food = startFood;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 thisLoc = new Vector2(this.transform.position.x, this.transform.position.z);
        if (!campHuman.activeSelf) {
            MeshRenderer humanRenderer = campHuman.GetComponent<MeshRenderer>();
            campHuman.SetActive(true);
            float mooseDist = Vector2.Distance(nearestMoose, thisLoc);
            float riverDist = Vector2.Distance(nearestRiver, thisLoc);
            float marshDist = Vector2.Distance(nearestMarsh, thisLoc);
            if (mooseDist < riverDist && mooseDist < marshDist) {
                destination = nearestMoose;
                humanRenderer.material = mooseMaterial;
            } else if (riverDist < mooseDist && riverDist < marshDist) {
                destination = nearestRiver;
                humanRenderer.material = fishMaterial;
            } else {
                destination = nearestMarsh;
                humanRenderer.material = marshMaterial;
            }
        }

        if (IsHumanAtTarget(destination)) {
            if (destination == thisLoc) {
                campHuman.SetActive(false);
                food = food + foodPerTrip;
            } else {
                destination = thisLoc;
            }
        } else {
            campHuman.transform.position = Vector3.MoveTowards(campHuman.transform.position, new Vector3(destination.x, 0.0f, destination.y), humanSpeed);
        }
        food--;
    }

    bool IsHumanAtTarget(Vector2 pTarget)
    {
        Vector2 humanLoc = new Vector2(campHuman.transform.position.x, campHuman.transform.position.z);
        if (Vector2.Distance(humanLoc, pTarget) < 1.0f) {
            return true;
        } else {
            return false;
        }
    }

    public void SetNearestRiver(Vector2 pRiver)
    {
        Debug.Log("Nearest river set to " + pRiver);
        nearestRiver = pRiver;
    }

    public void SetNearestMarsh(Vector2 pMarsh)
    {
        Debug.Log("Nearest marsh set to " + pMarsh);
        nearestMarsh = pMarsh;
    }

    public void SetNearestMoose(Vector2 pMoose)
    {
//        Debug.Log("Nearest moose set to " + pMoose);
        nearestMoose = pMoose;
    }

    public int GetFood()
    {
        return food;
    }

    void OnDestroy()
    {
        Object.Destroy(campHuman);
    }
}
