using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "New IceBarrierBuff", menuName = "RPG/Buff/IceBarrierBuff")]
public class IceBarrierBuff : ShieldBuff {
    [SerializeField] private Buff chillBuff;
    [SerializeField] private float chillRadius = 5f;

    public override void OnExpire() {
        base.OnExpire();
        ApplyChillDebuff();
    }

    private void ApplyChillDebuff() {
        List<Collider2D> hitTargets = new List<Collider2D>(Physics2D.OverlapCircleAll(new Vector2(targetCharacter.transform.position.x, targetCharacter.transform.position.y), chillRadius));

        List<Character> hitCharacters = hitTargets.Where(collider => {
            Character characterInRange = collider.gameObject.GetComponent<Character>();

            return characterInRange && targetCharacter.faction != characterInRange.faction;
        }).Select(collider => collider.gameObject.GetComponent<Character>()).ToList();

        foreach (Character hitCharacter in hitCharacters) {
            BuffController hitBuffController = hitCharacter.GetComponent<BuffController>();
            hitBuffController.ApplyBuffServerRpc("Chill");
        }
    }
}