using UnityEngine;

class ItemBox : GenericDamage
{
    public Sonic_PlayerStateMachine Ctx { get; set; }
    public GameItems Item;
    [SerializeField] private float value;
    public override void DealDamage(float _damage, Vector3 _knockback, int _strength)
    {
        base.DealDamage(_damage, _knockback, _strength);
        if (_strength < Strength)
        {
            switch (Item)
            {
                case GameItems.Ring:
                    {
                        Ctx.Chs.SetRings(Ctx.Chs.Rings + value);
                        break;
                    }

                case GameItems.Eggman:
                    {
                        if (Ctx.transform.TryGetComponent(out Sonic_DamageComponent s))
                        {
                            Vector3 kb = new Vector3(0, 10, -10);
                            s.DealDamage(value, kb, 9);
                        }
                        break;
                    }

                case GameItems.Invinciblity:
                    {
                        Ctx.InvinciblitiyState = value;
                        break;
                    }

                case GameItems.Shield_Classic:
                    {
                        Ctx.Chs.Shield = new ClassicShield();
                        Ctx.Chs.Shield.Start(Ctx);
                        break;
                    }
            }
        }
    }
}
public enum GameItems
{
    Ring,
    Eggman,
    Invinciblity,
    Shield_Bubble,
    Shield_Classic,
    Shield_Moon,
}
