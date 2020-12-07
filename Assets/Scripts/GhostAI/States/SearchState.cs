using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchState : State
{
    private float __timeout;
    private bool __isNearSpot;

    public SearchState(GameObject player, GameObject ghost, StateParams parameters)
        : base(player, ghost, parameters)
        {
            __timeout = parameters.searchTimeout;
            __isNearSpot = false;
        }

    public override StateType StateUpdate()
    {
        GameObject spot = GetNearestAvailableSpot();
        _agent.destination = spot.transform.position;

        __isNearSpot = !_agent.pathPending && _agent.remainingDistance < 2.0f;

        // __isNearSpot = Vector3.Distance(_ghost.transform.position, spot.transform.position) < 2.0f;
        if (__isNearSpot) _ghostSpotMapping.UpdateSpot(_ghost.name, spot.name);

        __timeout -= Time.deltaTime;

        return NextState();
    }

    protected override StateType NextState()
    {
        // Go to Flee state if ghost is inside player's FoV
        bool insideFov = Utils.IsTargetVisible(_player, _ghost, _camera.fieldOfView, Mathf.Infinity);
        if (insideFov)
            return StateType.Flee;

        // Go to Hide state if a spot is found
        if (__isNearSpot)
            return StateType.Hide;

        // Go to Roam state after a timeout if no spot is found
        if (__timeout < 0f)
            return StateType.Roam;

        // Keep the same state
        return StateType.Search;
    }

    public override void Exit()
    {
        base.Exit();
        __timeout = _parameters.searchTimeout;
    }

    private GameObject GetNearestAvailableSpot()
    {
        // TODO: Filter out already occupied spots

        GameObject[] spots = GameObject.FindGameObjectsWithTag("Prop");
        if (spots.GetLength(0) == 0) Debug.LogWarning("There are no hiding spots!");
        
        float minDist = Vector3.Distance(spots[0].transform.position, _ghost.transform.position);
        int minIdx = 0;

        for (int i = 1; i < spots.GetLength(0); ++i)
        {
            float dist = Vector3.Distance(spots[i].transform.position, _ghost.transform.position);
            if (dist < minDist)
            {
                minIdx = i;
                minDist = dist;
            }
        }

        return spots[minIdx];
    }
}
