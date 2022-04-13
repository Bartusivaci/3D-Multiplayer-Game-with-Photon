using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [SerializeField] Transform viewPoint;
    [SerializeField] float mouseSensitivity = 1f;
    [SerializeField] bool invertLook = false;
    [SerializeField] float playerMoveSpeed = 5f, playerRunSpeed = 8f;
    [SerializeField] CharacterController characterController;
    [SerializeField] float jumpForce = 12f, gravityMod = 2.5f;
    [SerializeField] Transform groundCheckPoint;
    [SerializeField] LayerMask groundLayers;
    [SerializeField] GameObject bulletImpact;
    // [SerializeField] float timeBetweenShots = 0.1f;
    [SerializeField] float maxHeatValue = 10f, /* heatPerShot = 1f, */ coolRate = 4f, overHeatCoolRate = 5f;
    [SerializeField] Gun[] gunArray;
    [SerializeField] float muzzleDisplayTime;
    [SerializeField] GameObject playerHitImpact;
    [SerializeField] int maxHealth = 100;
    [SerializeField] Animator anim;
    [SerializeField] GameObject playerModel;
    [SerializeField] Transform modelGunPoint, gunHolder;


    float verticalRotationStore;
    Vector2 mouseInput;
    Vector3 moveDirection, movement;
    float activeMoveSpeed;
    Camera cam;
    float yVelocity;
    bool isGrounded;
    float shotCounter;
    float heatCounter;
    bool isOverHeated;
    int selectedGun;
    float muzzleCounter;
    int currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        cam = Camera.main;

        UIController.instance.weaponHeatSlider.maxValue = maxHeatValue;

        //SwitchGun();

        photonView.RPC("SetGun", RpcTarget.All, selectedGun);

        currentHealth = maxHealth;

        //Transform newTransform = SpawnManager.instance.GetSpawnPoint();
        //transform.position = newTransform.position;
        //transform.rotation = newTransform.rotation;

        if (photonView.IsMine)
        {
            playerModel.SetActive(false);

            UIController.instance.healthLabel.text = currentHealth.ToString();
        }
        else
        {
            gunHolder.parent = modelGunPoint;
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (photonView.IsMine)
        {

            mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);

            verticalRotationStore += mouseInput.y;
            verticalRotationStore = Mathf.Clamp(verticalRotationStore, -60f, 60f);

            if (invertLook)
            {
                viewPoint.rotation = Quaternion.Euler(verticalRotationStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
            }
            else
            {
                viewPoint.rotation = Quaternion.Euler(-verticalRotationStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
            }

            moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

            if (Input.GetKey(KeyCode.LeftShift))
            {
                activeMoveSpeed = playerRunSpeed;
            }
            else
            {
                activeMoveSpeed = playerMoveSpeed;
            }

            yVelocity = movement.y;
            movement = ((transform.forward * moveDirection.z) + (transform.right * moveDirection.x)).normalized * activeMoveSpeed; //normalized prevents us to move faster in diagonal.
            movement.y = yVelocity;

            if (characterController.isGrounded)
            {
                movement.y = 0;
            }

            isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, 0.25f, groundLayers);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                movement.y = jumpForce;
            }

            movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;

            characterController.Move(movement * Time.deltaTime);

            if (gunArray[selectedGun].muzzleFlash.activeInHierarchy)
            {
                muzzleCounter -= Time.deltaTime;

                if (muzzleCounter <= 0)
                {
                    gunArray[selectedGun].muzzleFlash.SetActive(false);
                }
            }


            if (!isOverHeated)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Shoot();
                }

                if (Input.GetMouseButton(0) && gunArray[selectedGun].isAutomatic)  // for auto firing
                {
                    shotCounter -= Time.deltaTime;

                    if (shotCounter <= 0)
                    {
                        Shoot();
                    }
                }

                heatCounter -= coolRate * Time.deltaTime;
            }
            else
            {
                heatCounter -= overHeatCoolRate * Time.deltaTime;
                if (heatCounter <= 0)
                {
                    isOverHeated = false;
                    UIController.instance.overHeatedMessage.gameObject.SetActive(false);
                }
            }

            if (heatCounter < 0)
            {
                heatCounter = 0;
            }

            UIController.instance.weaponHeatSlider.value = heatCounter;

            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
            {
                selectedGun++;

                if (selectedGun >= gunArray.Length)
                {
                    selectedGun = 0;
                }

                //SwitchGun();
                photonView.RPC("SetGun", RpcTarget.All, selectedGun);

            }
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
            {
                selectedGun--;

                if (selectedGun < 0)
                {
                    selectedGun = gunArray.Length - 1;
                }

                //SwitchGun();
                photonView.RPC("SetGun", RpcTarget.All, selectedGun);

            }

            for (int i = 0; i < gunArray.Length; i++)
            {
                if (Input.GetKeyDown((i + 1).ToString()))
                {
                    selectedGun = i;
                    //SwitchGun();
                    photonView.RPC("SetGun", RpcTarget.All, selectedGun);
                }
            }

            anim.SetBool("grounded", isGrounded);
            anim.SetFloat("speed", moveDirection.magnitude);



            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else if (Cursor.lockState == CursorLockMode.None)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }

    }

    void LateUpdate()
    {
        if (photonView.IsMine)
        {
            cam.transform.position = viewPoint.position;
            cam.transform.rotation = viewPoint.rotation;
        }
    }


    void Shoot() {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // center point of the screen
        ray.origin = cam.transform.position;

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            // Debug.Log("We hit " + hit.collider.gameObject.name);

            if (hit.collider.gameObject.tag == "Player")
            {
                // Debug.Log("We hit " + hit.collider.gameObject.GetPhotonView().Owner.NickName);

                PhotonNetwork.Instantiate(playerHitImpact.name, hit.point, Quaternion.identity);

                hit.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All, photonView.Owner.NickName, gunArray[selectedGun].shotDamage, PhotonNetwork.LocalPlayer.ActorNumber); // run DealDamage() function on every player
            }
            else
            {

                GameObject bulletImpactObject = Instantiate(bulletImpact, hit.point + (hit.normal * 0.002f), Quaternion.LookRotation(hit.normal, Vector3.up));

                Destroy(bulletImpactObject, 10f);
            }
        }

        shotCounter = gunArray[selectedGun].timeBetweenShots;
        heatCounter += gunArray[selectedGun].heatPerShot;
        if(heatCounter >= maxHeatValue)
        {
            heatCounter = maxHeatValue;
            isOverHeated = true;

            UIController.instance.overHeatedMessage.gameObject.SetActive(true);
        }

        gunArray[selectedGun].muzzleFlash.SetActive(true);
        muzzleCounter = muzzleDisplayTime;
    }


    void SwitchGun()
    {
        foreach(Gun gun in gunArray)
        {
            gun.gameObject.SetActive(false);
        }

        gunArray[selectedGun].gameObject.SetActive(true);

        gunArray[selectedGun].muzzleFlash.SetActive(false);
    }

    [PunRPC]
    public void SetGun(int gunToSwitch)
    {
        if(gunToSwitch < gunArray.Length)
        {
            selectedGun = gunToSwitch;
            SwitchGun();
        }
    }

    [PunRPC] // runs the function in everyone at the same time
    public void DealDamage(string damager, int damageAmount, int actor)
    {
        TakeDamage(damager, damageAmount, actor);
    }


    public void TakeDamage(string damager, int damageAmount, int actor)
    {
        if (photonView.IsMine)
        {
            // Debug.Log(photonView.Owner.NickName + " has been hit by " + damager);

            currentHealth -= damageAmount;

            if(currentHealth <= 0)
            {
                currentHealth = 0;

                PlayerSpawner.instance.Die(damager);

                MatchManager.instance.UpdateStatsSend(actor, 0, 1);
            }

            UIController.instance.healthLabel.text = currentHealth.ToString();


        }
    }
}
