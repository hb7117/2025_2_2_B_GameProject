using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class DeliveryOrderSystem : MonoBehaviour
{
    [Header("주문 설정")]
    public float ordergenratelnterval = 15f;                  //주문 생성시간
    public int maxActiveOrders = 8;                           //최대 주문 숫자

    [Header("게임 상태")]
    public int totalOrderGenerated = 0;
    public int completedOrders = 0;
    public int expiredOrders = 0;

    //주문 리스트
    private List<DeliveryOrder> currentOrders = new List<DeliveryOrder>();

    //Builing 참조
    private List<Building> restaurants = new List<Building>();
    private List<Building> customers = new List<Building>();

    //Event 시스템
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
        FindAllBuilding();                       //건물 초기 셋팅
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

        Debug.Log($"음식점 {restaurants.Count}개 , 고객 {customers.Count} 개 발견");
    }

    void CreateNewOrdwe()
    {
        if (restaurants.Count == 0 || customers.Count == 0) return;

        //랜덤 음식점과 고객 선택
        Building randomRestaurant = restaurants[Random.Range(0, restaurants.Count)];
        Building randomCusromer = customers[Random.Range(0, customers.Count)];

        //같은 건물이면 다시 선택
        if (randomRestaurant == randomCusromer)
        {
            randomCusromer = customers[Random.Range(0, customers.Count)];
        }

        float reward = Random.Range(3000f, 8000f);

        DeliveryOrder newOrder = new DeliveryOrder(++totalOrderGenerated, randomRestaurant, randomCusromer, reward);

        currentOrders.Add(newOrder);
        orderEvents.OnNewOrderAdded?.Invoke(newOrder);

        void PickupOrder(DeliveryOrder order)                       //픽업 함수
        {
            order.state = OrderState.PickedUp;
            orderEvents.OnOrderPickUp?.Invoke(order);
        }

        void CompleteOrder(DeliveryOrder order)                     //배달 완료 함수
        {
            order.state = OrderState.Completed;
            completedOrders++;

            //보상 지급
            if(driver != null)
            {
                driver.AddMoney(order.reward);
            }

            //완료된 주문 제거

            currentOrders.Remove(order);
            orderEvents.OnOrderCompleted?.Invoke(order);
        }
    }

    void ExpireOrder(DeliveryOrder order)                    //주문 취소 소멸
    {
        order.state = OrderState.Expired;
        expiredOrders++;

        currentOrders.Remove(order);
        orderEvents.OnOrderExpired?.Invoke(order);
    }

    //UI 정보 제공
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
