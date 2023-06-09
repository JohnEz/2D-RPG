﻿using UnityEngine;
using System.Collections;
using Unity.Netcode;
using System;

public class NetworkHealth : NetworkBehaviour {

    [HideInInspector]
    public NetworkVariable<int> HitPoints = new NetworkVariable<int>();

    [HideInInspector]
    public NetworkVariable<int> ShieldPoints = new NetworkVariable<int>();

    // public subscribable event to be invoked when HP has been fully depleted
    public event Action HitPointsDepleted;

    // public subscribable event to be invoked when HP has been replenished
    public event Action HitPointsReplenished;

    private void OnEnable() {
        HitPoints.OnValueChanged += HitPointsChanged;
    }

    private void OnDisable() {
        HitPoints.OnValueChanged -= HitPointsChanged;
    }

    private void HitPointsChanged(int previousValue, int newValue) {
        if (previousValue > 0 && newValue <= 0) {
            // newly reached 0 HP
            HitPointsDepleted?.Invoke();
        } else if (previousValue <= 0 && newValue > 0) {
            // newly revived
            HitPointsReplenished?.Invoke();
        }
    }
}