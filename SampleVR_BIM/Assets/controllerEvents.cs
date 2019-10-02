using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tridify;

public class controllerEvents : MonoBehaviour
{
    public bool enableTeleporter;

    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device controller;

    public GameObject laserPrefab;
    private GameObject laser;
    private Transform laserTransform;
    private Vector3 hitPoint;
    public GameObject mirroredCube;

    public GameObject hitPointObj;
    public GameObject cameraRig;
    public Material greenMat, redMat;
    public Text bimText;

    private void ShowLaser(RaycastHit hit) {
        mirroredCube.SetActive(false);
        laser.SetActive(true);
        laserTransform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, .5f);
        laserTransform.LookAt(hitPoint);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, hit.distance);
    }

    void teleportOnFloor() {
        laserTransform.GetComponent<MeshRenderer>().material = redMat;
        if (hitPointObj.activeInHierarchy == true) {
            hitPointObj.SetActive(false);
        }
    }

    void displayCanvas() {
        if (controller.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu)) {
            bimText.transform.parent.gameObject.SetActive(!bimText.transform.parent.gameObject.activeInHierarchy);
        }
    }

    void displayBIMInformation(RaycastHit hit) {
        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("BIM")) {
            if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
                bimText.text = "Name:" + hit.transform.name;
                if (hit.transform.GetComponent<IfcPropertySet>() != null) {
                    bimText.text += "\n Property set";
                    for (int i = 0; i < hit.transform.GetComponent<IfcPropertySet>().Attributes.Length; i++) {
                        bimText.text += "\n" + hit.transform.GetComponent<IfcPropertySet>().Attributes[i].Name + ":" + hit.transform.GetComponent<IfcPropertySet>().Attributes[i].Value;
                    }
                    //bimText.text += "\n Property set..";
                } else {
                    bimText.text += "\n No additional data available.";
                }
            }
        }
    }

    public void teleportOnFloor(RaycastHit hit) {
        // When the laser hits the floor, a small bubble(hitPointObj) showing the user the location they'll teleport to.
        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Floor")) {
            laserTransform.GetComponent<MeshRenderer>().material = greenMat;
            if (hitPointObj.activeInHierarchy == false) {
                hitPointObj.SetActive(true);
            }
            hitPointObj.transform.position = hit.point;
            // If trigger is pressed, the user will teleport to the given location
            if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
                print("Teleported to:" + hit.transform.position);
                //Vector3 offset = cameraRig.transform.position - this.transform.position;
                Vector3 offset = Vector3.zero;
                cameraRig.transform.position = new Vector3(hitPoint.x + offset.x, hitPoint.y, hitPoint.z + offset.z);
            }
        } else {
            //Laser changes from green to red
            laserTransform.GetComponent<MeshRenderer>().material = redMat;
            if (hitPointObj.activeInHierarchy == true) {
                hitPointObj.SetActive(false);
            }
        }
    }


    private void Awake() {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    // Start is called before the first frame update
    void Start() {
        laser = Instantiate(laserPrefab);
        laser.transform.SetParent(this.transform);
        laserTransform = laser.transform;
    }

    // Update is called once per frame
    void Update() {
        controller = SteamVR_Controller.Input((int)trackedObj.index);
        displayCanvas();
        Ray ray = Camera.main.ScreenPointToRay(trackedObj.transform.position);
        RaycastHit hit;
        if (Physics.Raycast(trackedObj.transform.position, trackedObj.transform.forward, out hit, 100)) {
            hitPoint = hit.point;
            displayBIMInformation(hit);
            if (enableTeleporter) {
                teleportOnFloor(hit);
            }
            ShowLaser(hit);
        } else {
            teleportOnFloor();
        }
    }
}
