using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    private Quaternion targetRotation;  // 목표 회전 각도
    public float rotationSpeed = 5f;    // 회전 속도

    void Start()
    {
        // 초기화
        targetRotation = transform.rotation;
    }

    void Update()
    {
        // 화살표 입력 감지
        //A,D키 추가

        if (Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.A))
        {
            targetRotation *= Quaternion.Euler(0, -90, 0);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            targetRotation *= Quaternion.Euler(0, 90, 0);
        }

        // 부드럽게 회전
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}