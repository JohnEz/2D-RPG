using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionBarManager : Singleton<ActionBarManager> {
    private Character myCharacter;

    [SerializeField] private List<AbilityIcon> abilityIcons;

    private void Start() {
        foreach (AbilityIcon abilityIcon in abilityIcons) {
            abilityIcon.gameObject.SetActive(false);
        }
    }

    public void SetCharacter(Character character) {
        myCharacter = character;

        int index = 0;
        foreach (Ability ability in character.abilities) {
            abilityIcons[index].gameObject.SetActive(true);
            abilityIcons[index].SetAbility(ability);
            index++;
        }
    }
}