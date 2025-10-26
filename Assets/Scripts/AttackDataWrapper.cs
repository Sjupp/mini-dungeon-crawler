public class AttackDataWrapper
{
    public AttackDataSO AttackDataSO = null;
    public Item UsedItem = null;

    public AttackDataWrapper(AttackDataSO attackDataSO, Item usedItem)
    {
        AttackDataSO = attackDataSO;
        UsedItem = usedItem;
    }
}
