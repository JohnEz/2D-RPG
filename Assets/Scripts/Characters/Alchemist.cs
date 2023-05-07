using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Alchemist : Character {
    // Toxic Bolt

    public Ability toxicBoltObject;
    public GameObject toxicBoltPrefab;
    public Buff toxicBoltBuff;

    public Ability healingPotionObject;
    public GameObject healingPotionPrefab;
    public Buff healingPotionBuff;

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        SetBaseStats(190, 4f);

        Ability toxicBolt = Instantiate(toxicBoltObject);
        Ability healingPotion = Instantiate(healingPotionObject);

        toxicBolt.OnCastComplete = () => ToxicBoltCast(toxicBolt);
        toxicBolt.OnHit = (Character hitCharacter, Vector3 hitLocation) => OnToxicBoltHit(toxicBolt, hitCharacter, hitLocation);
        abilities.Add(toxicBolt);

        healingPotion.OnCastComplete = () => HealingPotionCast(healingPotion);
        healingPotion.OnHit = (Character hitCharacter, Vector3 hitLocation) => OnHealingPotionHitLocation(healingPotion, hitCharacter, hitLocation);
        abilities.Add(healingPotion);
    }

    private void ToxicBoltCast(Ability toxicBolt) {
        //GameObject arrow = Instantiate(toxicBoltPrefab);
        //Projectile projectileController = arrow.GetComponent<Projectile>();

        //projectileController.Initialise(faction, toxicBolt.OnHit);
        //projectileController.MoveInDirection(transform.position, unitController.GetAimDirection());
    }

    private void OnToxicBoltHit(Ability toxicBolt, Character hitCharacter, Vector3 hitLocation) {
        if (!hitCharacter) {
            return;
        }

        BuffController hitBuffController = hitCharacter.GetComponent<BuffController>();

        int damageMin = 10;
        int damageMax = 14;

        hitCharacter.TakeDamage(Random.Range(damageMin, damageMax + 1));
        hitBuffController.ApplyBuffServerRpc("Toxic");
    }

    private void HealingPotionCast(Ability healingPoition) {
        //GameObject potion = Instantiate(healingPotionPrefab);
        //Projectile projectileController = potion.GetComponent<Projectile>();

        //projectileController.Initialise(faction, healingPoition.OnHit);
        //projectileController.MoveToTarget(transform.position, unitController.aimPosition);
    }

    private void OnHealingPotionHitLocation(Ability healingPoition, Character hitCharacter, Vector3 hitLocation) {
        List<Collider2D> hitTargets = new List<Collider2D>(Physics2D.OverlapCircleAll(new Vector2(hitLocation.x, hitLocation.y), healingPoition.radius));

        List<Character> hitCharacters = hitTargets.Where(collider => {
            Character characterInRange = collider.gameObject.GetComponent<Character>();

            return characterInRange && faction == characterInRange.faction;
        }).Select(collider => collider.gameObject.GetComponent<Character>()).ToList();

        foreach (Character character in hitCharacters) {
            OnHealingPotionHitCharacter(character);
        }
    }

    private void OnHealingPotionHitCharacter(Character hitCharacter) {
        if (!hitCharacter) {
            return;
        }

        BuffController hitBuffController = hitCharacter.GetComponent<BuffController>();

        int healingMin = 10;
        int healingMax = 14;

        hitCharacter.ReceiveHealing(Random.Range(healingMin, healingMax + 1));
        hitBuffController.ApplyBuffServerRpc("Revitalize");
    }
}