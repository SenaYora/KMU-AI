using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Random = UnityEngine.Random;

public class FP_0000000_Agent : Agent
{
    public GameObject spawnPointsGameObject;
    public Transform target;

    private Rigidbody _rigidbody;

    private List<Vector3> _spawnPoints;
    private Vector3 m_Velocity = Vector3.zero;
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;

    private float oldDistance = Mathf.Infinity;

    private float timeBegining;

    public override void Initialize()
    {
        _spawnPoints = new List<Vector3>();
        _rigidbody = GetComponent<Rigidbody>();
        foreach (Transform child in spawnPointsGameObject.transform)
        {
            _spawnPoints.Add(child.localPosition);
        }
        timeBegining = Time.time;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(transform.localPosition);

        sensor.AddObservation(_rigidbody.velocity.x);
        sensor.AddObservation(_rigidbody.velocity.z);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.y = actionBuffers.ContinuousActions[1];

        Vector3 targetVelocity = new Vector3(actionBuffers.ContinuousActions[0] * 10f, 0f, actionBuffers.ContinuousActions[1] * 10f);
        _rigidbody.velocity = Vector3.SmoothDamp(_rigidbody.velocity, targetVelocity, ref m_Velocity,
        m_MovementSmoothing);

        float distance = Vector3.Distance(transform.localPosition, target.transform.localPosition);

        if (distance < 1.4f)
        {
            Debug.Log("Reset: Win");
            SetReward(4f);
            EndEpisode();
        }
        if (transform.localPosition.y < 0f)
        {
            Debug.Log("Reset: Fall");
            SetReward(-1f);
            EndEpisode();
        }
        if (distance < oldDistance)
        {
            SetReward(0.05f);
        }
        // if (Time.time > timeBegining + 1000)
        // {
        //     Debug.Log("Reset: Time");
        //     SetReward(-1f);
        //     EndEpisode();
        // }
        SetReward(-0.01f);
        oldDistance = distance;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    public override void OnEpisodeBegin()
    {
        Reset();
    }

    private void Reset()
    {
        _rigidbody.velocity = Vector3.zero;
        transform.localPosition = _spawnPoints[Random.Range(0, _spawnPoints.Count)];
        timeBegining = Time.time;
    }
}
