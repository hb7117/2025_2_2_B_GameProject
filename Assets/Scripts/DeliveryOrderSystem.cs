using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class DeliveryOrderSystem : MonoBehaviour
{
    [Header("�ֹ� ����")]
    public float ordergenratelnterval = 15f;                  //�ֹ� �����ð�
    public int maxActiveOrders = 8;                           //�ִ� �ֹ� ����

    [Header("���� ����")]
    public int totalOrderGenerated = 0;
    public int completedOrders = 0;
    public int expiredOrders = 0;

    //�ֹ� ����Ʈ
    private List<DeliveryOrder> currentOrders = new List<DeliveryOrder>();

    //Builing ����
    private List<Building> restaurants = new List<Building>();
    private List<Building> customers = new List<Building>();

    //Event �ý���
    [System.Serializable]
    public class OrderSystemEvents
    {
        public UnityEvent<DeliveryOrder> OnNewOrderAdded;
        public UnityEvent<DeliveryOrder> OnOrderPickUp;
        public UnityEvent<DeliveryOrder> OnOrderCompleted;
        public UnityEvent<DeliveryOrder> OnOrderExpired;
    }

    public OrderSystemEvents orderEvents;
    private DeliveryDriver driver;

    void Start()
    {
        FindAllBuilding();                       //�ǹ� �ʱ� ����
    }

    void FindAllBuilding()
    {
        Building[] allBuildings = FindObjectsOfType<Building>();

        foreach (Building building in allBuildings)
        {
            if (building.BuildingType == BuildingType.Restaurant)
            {
                restaurants.Add(building);
            }
            else if(building.BuildingType == BuildingType.Customer)
            {
                customers.Add(building);
            }
        }

        Debug.Log($"������ {restaurants.Count}�� , �� {customers.Count} �� �߰�");
    }

    void CreateNewOrdwe()
    {
        if (restaurants.Count == 0 || customers.Count == 0) return;

        //���� �������� �� ����
        Building randomRestaurant = restaurants[Random.Range(0, restaurants.Count)];
        Building randomCusromer = customers[Random.Range(0, customers.Count)];

        //���� �ǹ��̸� �ٽ� ����
        if (randomRestaurant == randomCusromer)
        {
            randomCusromer = customers[Random.Range(0, customers.Count)];
        }

        float reward = Random.Range(3000f, 8000f);

        DeliveryOrder newOrder = new DeliveryOrder(++totalOrderGenerated, randomRestaurant, randomCusromer, reward);

        currentOrders.Add(newOrder);
        orderEvents.OnNewOrderAdded?.Invoke(newOrder);

        void PickupOrder(DeliveryOrder order)                       //�Ⱦ� �Լ�
        {
            order.state = OrderState.PickedUp;
            orderEvents.OnOrderPickUp?.Invoke(order);
        }

        void CompleteOrder(DeliveryOrder order)                     //��� �Ϸ� �Լ�
        {
            order.state = OrderState.Completed;
            completedOrders++;

            //���� ����
            if(driver != null)
            {
                driver.AddMoney(order.reward);
            }

            //�Ϸ�� �ֹ� ����

            currentOrders.Remove(order);
            orderEvents.OnOrderCompleted?.Invoke(order);
        }
    }

    void ExpireOrder(DeliveryOrder order)                    //�ֹ� ��� �Ҹ�
    {
        order.state = OrderState.Expired;
        expiredOrders++;

        currentOrders.Remove(order);
        orderEvents.OnOrderExpired?.Invoke(order);
    }

    //UI ���� ����
    public List<DeliveryOrder> GetDeliveryOrders()
    {
        return new List<DeliveryOrder>(currentOrders);
    }

    public int GetPickWaitingCount()
    {
        int count = 0;
        foreach (DeliveryOrder order in currentOrders)
        {
            if (order.state == OrderState.WaitingPickup) count++;
        }
        return count;
    }
}
