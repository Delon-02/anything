using System;
using System.Collections.Generic;

namespace MatchaBakery.Backend
{
    public static class ActionTypes
    {
        public const string CompleteOrder = "completeOrder";
        public const string BuyIngredient = "buyIngredient";
        public const string ClaimDaily = "claimDaily";
    }

    [Serializable] public class OrderLine { public string id; public int qty; }
    [Serializable] public class CompleteOrderPayload
    {
        public string orderId;
        public long completedAtMs;
        public List<OrderLine> lines = new List<OrderLine>();
    }
    [Serializable] public class BuyIngredientPayload { public string ingredientId; public int packs; }
    [Serializable] public class EmptyPayload { }

    [Serializable] public class ActionResultDto
    {
        public string actionId; public bool ok; public bool duplicate;
        public string code; public string message;
    }
    [Serializable] public class StockEntry { public string id; public int qty; }
    [Serializable] public class ServerSnapshot
    {
        public long money; public long totalEarned; public long xp;
        public List<StockEntry> stock = new List<StockEntry>();
    }
    [Serializable] public class BatchResponse
    {
        public List<ActionResultDto> results = new List<ActionResultDto>();
        public ServerSnapshot state = new ServerSnapshot();
    }

    [Serializable] public class PendingAction
    {
        public string actionId;
        public string type;
        public string payloadJson;
        public long createdAtUtcMs;
        public int attempts;
        public long nextTryUtcMs;
    }
    [Serializable] public class PendingActionFile
    {
        public int version = 1;
        public string ownerUid;
        public List<PendingAction> items = new List<PendingAction>();
    }

    public enum CallStatus { Success, Offline, Timeout, RetryLater, AuthRequired, Rejected, Unavailable }

    public class CallOutcome
    {
        public CallStatus Status;
        public string Detail;
        public BatchResponse Response;
    }
}