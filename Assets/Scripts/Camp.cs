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
    List<GameObject> campHumans = new List<GameObject>();
    public float humanSpeed;
    [SerializeField] Material mooseMaterial;
    [SerializeField] Material fishMaterial;
    [SerializeField] Material marshMaterial;
    [SerializeField] Material forageMaterial;
    [SerializeField] int startFood;
    [SerializeField] int foodPerTrip;
    float mooseDist, riverDist, marshDist;
    Vector2 thisLoc;
    public int humanInterval;
    int lastHuman;
    LocalLandscapeImport land;



    // Start is called before the first frame update
    void Start()
    {
        land = GameObject.Find("LocalSceneManager").GetComponent<LocalLandscapeImport>();

        thisLoc = new Vector2(this.transform.position.x, this.transform.position.z);
        riverDist = Vector2.Distance(nearestRiver, thisLoc);
        marshDist = Vector2.Distance(nearestMarsh, thisLoc);
        food = startFood;
        lastHuman = startFood;
    }

    // Update is called once per frame
    void Update()
    {
/*        mooseDist = Vector2.Distance(nearestMoose, thisLoc);
        List<GameObject> humansToDelete = new List<GameObject>();


        foreach (var thisHumanGO in campHumans)
        {
            Human thisHuman = thisHumanGO.GetComponent<Human>();
            if (IsHumanAtTarget(thisHumanGO, thisHuman.GetDestination())) {
                Debug.Log("Human has reached target at " + thisHuman.GetDestination());
                if (thisHuman.GetDestination() == thisLoc) {
                    Debug.Log("Human back at camp");
                    if (land.IsSnow((int)thisLoc.x, (int)thisLoc.y)) {
                        food = food + (foodPerTrip / 2);
                    } else {
                        food = food + foodPerTrip;
                    }
                    lastHuman = food;
                    humansToDelete.Add(thisHumanGO);
                } else {
                    thisHuman.SetDestination(thisLoc);
                    Debug.Log("Human heading back to camp at " + thisLoc);
                }
            } else {
//                Debug.Log("Human premove is at " + thisHumanGO.transform.position);
                thisHumanGO.transform.position = Vector3.MoveTowards(thisHumanGO.transform.position, new Vector3(thisHuman.GetDestination().x, land.GetLocationDepth((int)thisHuman.GetDestination().x, (int)thisHuman.GetDestination().y), thisHuman.GetDestination().y), humanSpeed * Time.deltaTime);
//                Debug.Log("Human postmove is at " + thisHumanGO.transform.position);
            }
        }

        foreach (var thisHumanDelete in humansToDelete)
        {
            campHumans.Remove(thisHumanDelete);
            Destroy(thisHumanDelete);
            Debug.Log("Removed a human");
        }

        if (campHumans.Count < 2 && mooseDist > 1 && mooseDist < 200) {
            GenerateHuman(false);
            lastHuman = food;
        } else if (food < (startFood / 2) && campHumans.Count < 5 && food < (lastHuman - humanInterval)) {
            GenerateHuman(true);
            lastHuman = food;
        }
        food--;*/
    }

    bool IsHumanAtTarget(GameObject thisHuman, Vector2 pTarget)
    {
        Vector2 humanLoc = new Vector2(thisHuman.transform.position.x, thisHuman.transform.position.z);
        if (Vector2.Distance(humanLoc, pTarget) < 1.0f) {
            return true;
        } else {
            return false;
        }
    }

    void GenerateHuman(bool pForager)
    {
        GameObject thisHumanGO = Instantiate(human, this.transform.position, Quaternion.identity);
        Human thisHuman = thisHumanGO.GetComponent<Human>();
        MeshRenderer humanRenderer = thisHumanGO.GetComponent<MeshRenderer>();

        if (!pForager) {
            if (mooseDist < riverDist && mooseDist < marshDist) {
                thisHuman.SetDestination(nearestMoose);
                Debug.Log("Setting human destination to moose at " + nearestMoose);
                humanRenderer.material = mooseMaterial;
            } else if (riverDist < mooseDist && riverDist < marshDist) {
                thisHuman.SetDestination(nearestRiver);
                Debug.Log("Setting human destination to river at " + nearestRiver);
                humanRenderer.material = fishMaterial;
            } else {
                thisHuman.SetDestination(nearestMarsh);
                Debug.Log("Setting human destination to marsh at " + nearestMarsh);
                humanRenderer.material = marshMaterial;
            }
        } else {
            thisHuman.SetDestination(GetForagingDestination());
            Debug.Log("Setting human destination to forage at " + thisHuman.GetDestination());
            humanRenderer.material = forageMaterial;
        }
        campHumans.Add(thisHumanGO);
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

    public void ResetFood()
    {
        food = startFood;
    }

    void OnDestroy()
    {
        foreach (var thisHumanGO in campHumans)
        {
            Object.Destroy(thisHumanGO);
        }
    }

    Vector2 GetForagingDestination()
    {
        int foragingRadius = 75;
        int x = Random.Range(0 - foragingRadius, foragingRadius);
        int y = Random.Range(0 - foragingRadius, foragingRadius);
        int endX = (int) thisLoc.x + x;
        int endY = (int) thisLoc.y + y;
        if (endX < 0)
        {
            endX = 0;
        }
        if (endY < 0)
        {
            endY = 0;
        }
        if (endX > 511)
        {
            endX = 511;
        }
        if (endY > 511)
        {
            endY = 511;
        }
        return new Vector2(endX, endY);
    }
}
