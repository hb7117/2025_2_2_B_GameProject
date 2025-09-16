using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DeliveryDriver : MonoBehaviour
{
    [Header("��޿� ����")]
    public float moveSpeed = 8f;
    public float rotationSpeed = 10.0f;

    [Header("����")]
    public float currentMoney = 0;
    public float batteryLevel = 100f;
    public int deliveryCount = 0;

    //Event �ý���
    [System.Serializable]
    public class DriverEvents                                       //�پ��� �̺�Ʈ ���� Ŭ���� ���� 
    {
        [Header("�̵� Event")]
        public UnityEvent OnMoveStarted;
        public UnityEvent OnMoveStoped;

        [Header("���� ��ȭ Event")]
        public UnityEvent<float> OnMoneyChanged;
        public UnityEvent<float> OnBatteryChanged;
        public UnityEvent<int> OnDeliveryCountChanged;

        [Header("��� Event")]
        public UnityEvent OnLowBattery;
        public UnityEvent OnLowBatteryEmpty;
        public UnityEvent OnDeliveryCompleted;
    }

    public DriverEvents driverEvents;

    public bool isMoving = false;

    // Start is called before the first frame update
    void Start()
    {
        //�ʱ� ���� Event �߻�
        driverEvents.OnMoneyChanged?.Invoke(currentMoney);
        driverEvents.OnBatteryChanged?.Invoke(batteryLevel);
        driverEvents.OnDeliveryCountChanged?.Invoke(deliveryCount);
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        UpdateBattery();
    }

    void HandleMovement()
    {
        //���͸� üũ
        if(batteryLevel <= 0)
        {
            if(isMoving)
            {
                StopMoving();
            }
            return;
        }

        //�Է� �ޱ�
        float horizontal = Input.GetAxis("Horizontal");
        float verical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0, verical);

        if(moveDirection.magnitude > 0.1f)
        {
            if(!isMoving)
            {
                StartMoving();
            }

            //�̵� ó��
            moveDirection = moveDirection.normalized;
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

            //ȸ�� ó��
            if(moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation , targetRotation, rotationSpeed * Time.deltaTime);    
            }
            ChangeBattery(-Time.deltaTime * 3.0f);      //�̵��Ҷ����� ���͸� �Ҹ� 
        }
        else
        {
            if(isMoving)
            {
                StopMoving();
            }
        }
    }

    void ChangeBattery(float amount)
    {
        float oldBattery = batteryLevel;
        batteryLevel += amount;
        batteryLevel = Mathf.Clamp(batteryLevel, 0, 100);

        //���͸� ��ȭ Event �߻�
        driverEvents.OnBatteryChanged?.Invoke(batteryLevel);

        //���͸� ���¿� ���� ���
        if(oldBattery > 20f && batteryLevel <= 20f)
        {
            driverEvents.OnLowBattery?.Invoke();                //���͸� ���� ����
        }
        if (oldBattery > 0f && batteryLevel <= 0f)
        {
            driverEvents.OnLowBatteryEmpty?.Invoke();                //���͸� ����
        }
    }

    void StartMoving()
    {
        isMoving = true;
        driverEvents.OnMoveStarted?.Invoke();       //���콺 ����
    }

    void StopMoving()
    {
        isMoving = false;
        driverEvents.OnMoveStoped?.Invoke();       //���콺 ����
    }

    void UpdateBattery()
    {
        //�ƹ��͵� ���ص� ���ݾ� ���͸� �Ҹ�
        if(batteryLevel > 0)
        {
            ChangeBattery(-Time.deltaTime * 0.5f);
        }
    }

    public void AddMoney(float amount)
    {
        currentMoney += amount;
        driverEvents.OnMoneyChanged?.Invoke(currentMoney);          //�� ȹ�� �� �̺�Ʈ ó�� 
    }

    public void CompleteDelivery()                                  //��� �Ϸ� �Լ� 
    {
        deliveryCount++;
        float reward = Random.Range(3000, 8000);

        AddMoney(reward);
        driverEvents.OnDeliveryCountChanged?.Invoke(deliveryCount);
        driverEvents.OnDeliveryCompleted?.Invoke();
    }
    public void ChargeBattery()
    {
        ChangeBattery(100f -  batteryLevel);            //���͸� ����
    }

    public string GetStatusText()
    {
        return $"�� : {currentMoney:F0} �� | ���͸� : {batteryLevel:F1}% | ��� : {deliveryCount} ��";
    }

    public bool CanMove()
    {
        return batteryLevel > 0;
    }
}