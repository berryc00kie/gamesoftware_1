using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    private Quaternion targetRotation;  // ��ǥ ȸ�� ����
    public float rotationSpeed = 5f;    // ȸ�� �ӵ�

    void Start()
    {
        // �ʱ�ȭ
        targetRotation = transform.rotation;
    }

    void Update()
    {
        // ȭ��ǥ �Է� ����
        //A,DŰ �߰�

        if (Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.A))
        {
            targetRotation *= Quaternion.Euler(0, -90, 0);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            targetRotation *= Quaternion.Euler(0, 90, 0);
        }

        // �ε巴�� ȸ��
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}