using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
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
    [SerializeField] float timeBetweenShots = 0.1f;
    [SerializeField] float maxHeatValue = 10f, heatPerShot = 1f, coolRate = 4f, overHeatCoolRate = 5f;


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
    

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
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

        moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"),0f,Input.GetAxisRaw("Vertical"));

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



        if (!isOverHeated)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }

            if (Input.GetMouseButton(0))  // for auto firing
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
            if(heatCounter <= 0)
            {
                isOverHeated = false;
            }
        }

        if(heatCounter < 0)
        {
            heatCounter = 0;
        }

        



        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if(Cursor.lockState == CursorLockMode.None)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

    }

    void LateUpdate()
    {
        cam.transform.position = viewPoint.position;
        cam.transform.rotation = viewPoint.rotation;
    }


    void Shoot() {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // center point of the screen
        ray.origin = cam.transform.position;

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("We hit " + hit.collider.gameObject.name);

            GameObject bulletImpactObject = Instantiate(bulletImpact, hit.point + (hit.normal * 0.002f), Quaternion.LookRotation(hit.normal, Vector3.up));

            Destroy(bulletImpactObject, 10f);
        }

        shotCounter = timeBetweenShots;
        heatCounter += heatPerShot;
        if(heatCounter >= maxHeatValue)
        {
            heatCounter = maxHeatValue;
            isOverHeated = true;
        }
    }
}
