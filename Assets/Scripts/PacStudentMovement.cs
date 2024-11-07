using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PacStudentMovement : MonoBehaviour
{

    Animator pacStudentAnimator;
    AudioSource movementAudio;
    public bool IsMoving;
    public float speed;
    GameObject PacStudent;
    Tilemap tile;

    Vector3[] corners = new Vector3[]
{
    new Vector3(-10.2f, 6.7f, 0), // Top-left
    new Vector3(-7.8f, 6.7f, 0), // Top-right
    new Vector3(-7.8f, 4.7f, 0), // Bottom-right
    new Vector3(-10.2f, 4.7f, 0)  // Bottom-left
};


    int currentCorner = 0;
    private void MovePacStudent()
    {
        Vector3 targetPosition = corners[currentCorner];
        PacStudent.transform.position = Vector3.MoveTowards(PacStudent.transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector3.Distance(PacStudent.transform.position, targetPosition) < 0.1f)
        {
            if (currentCorner != 0)
            {
                pacStudentAnimator.SetBool("turned", true);
            }
            currentCorner = (currentCorner + 1) % corners.Length;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        pacStudentAnimator = GetComponent<Animator>();
        pacStudentAnimator.SetTrigger("Start");
        movementAudio = GetComponent<AudioSource>();
        pacStudentAnimator.SetBool("turned", false);
        PacStudent = this.gameObject;


    }

    // Update is called once per frame
    void Update()
    {
        pacStudentAnimator.SetBool("turned", false);
        MovePacStudent();


        if (PacStudentIsMoving())
        {
            pacStudentAnimator.SetBool("IsMoving", true);
            if (!movementAudio.isPlaying)
            {
                movementAudio.Play();
            }
        }
        else
        {
            pacStudentAnimator.SetBool("IsMoving", false);
            movementAudio.Stop();
        }
    }

    bool PacStudentIsMoving()
    {
        return PacStudent.transform.position != corners[currentCorner];
    }

}
