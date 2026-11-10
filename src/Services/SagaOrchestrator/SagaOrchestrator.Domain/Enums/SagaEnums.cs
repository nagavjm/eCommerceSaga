namespace SagaOrchestrator.Domain.Enums;

public enum SagaStateEnum
{
    Started = 1,
    ReservingStock = 2,
    StockReserved = 3,
    ProcessingPayment = 4,
    PaymentCompleted = 5,
    ConfirmingOrder = 6,
    ConfirmingStockDeduction = 7,
    CreatingShipment = 8,
    SendingNotification = 9,
    Completed = 10,
    CompensatingInventory = 11,
    CompensatingOrder = 12,
    Failed = 13,
    EscalatedForManualIntervention = 14
}

public enum CompensationStatus
{
    None = 1,
    InProgress = 2,
    Completed = 3,
    Failed = 4
}
