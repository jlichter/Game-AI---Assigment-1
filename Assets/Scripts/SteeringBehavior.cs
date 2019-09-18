using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the place to put all of the various steering behavior methods we're going
/// to be using. Probably best to put them all here, not in NPCController.
/// </summary>

public class SteeringBehavior : MonoBehaviour {

    // The agent at hand here, and whatever target it is dealing with
    public NPCController agent;
    public NPCController target;

    // Below are a bunch of variable declarations that will be used for the next few
    // assignments. Only a few of them are needed for the first assignment.

    // For pursue and evade functions
    public float maxPrediction;
    public float maxAcceleration;

    // For arrive function
    public float maxSpeed;
    public float targetRadiusL;
    public float slowRadiusL;
    public float timeToTarget;

    // For Face function
    public float maxRotation;
    public float maxAngularAcceleration;
    public float targetRadiusA;
    public float slowRadiusA;

    // For wander function
    public float wanderOffset;
    public float wanderRadius;
    public float wanderRate;
    private float wanderOrientation;

    // Holds the path to follow
    public GameObject[] Path;
    public int current = 0;

    protected void Start() {
        agent = GetComponent<NPCController>();
        //wanderOrientation = agent.orientation;
    }

    public Vector3 Seek() {
        Vector3 direction = target.position - agent.position;
        direction.Normalize();
        direction *= maxAcceleration;
        return direction;
    }


    public Vector3 Flee() {
        Vector3 direction = target.position - agent.position;
        direction.Normalize();
        direction *= maxAcceleration;
        return direction;
    }

    /*
     * getSteering() calculates a surrogate target
     * and returns the target's position
     */
    public Vector3 getSteering() {
        // work out the distance to target  
        Vector3 direction = target.position - agent.position;
        float distance = direction.magnitude;

        // work out our current speed
        float speed = agent.velocity.magnitude;

        // for our end prediction
        float prediction;

        // check if speed is too small to give a reasonable prediction time 
        if (speed <= distance / maxPrediction) {
            prediction = maxPrediction;
        } else {
            prediction = distance / speed;
        }
        // get target's new position 
        Vector3 targetPos = target.position + target.velocity * prediction;

        return targetPos; // return the position
    }
    /*
     * Pursue() calls getSteering() and calculates the
       direction from the character to the target and 
       requests a velocity along this line
     */
    public Vector3 Pursue() {

        // call to getSteering()
        Vector3 targetPosition = getSteering();
        // get the direction to the target 
        Vector3 steering = targetPosition - agent.position;
        // the velocity is along this direction, at full speed 
        steering.Normalize();
        steering *= maxAcceleration;
        //output the steering
        return steering;

    }
    /*
     * Evade() calls getSteering() and calculates the
       direction from the character to the target and 
       requests a velocity in the opposite direction
     */
    public Vector3 Evade() {
        // call to getSteering()
        Vector3 targetPosition = getSteering();
        // get the direction away from the target
        Vector3 steering = agent.position - targetPosition;
        // the velocity is along this direction, at full speed 
        steering.Normalize();
        steering *= maxAcceleration;
        //output the steering
        return steering;

    }


    private Vector3 orientationVector(float angle) {
        return new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
    }


    public Vector3 Wander() {
        wanderOrientation += (Random.value - Random.value) * wanderRate;
        target.orientation = agent.orientation + wanderOrientation;
        target.position = agent.position + orientationVector(agent.orientation) * wanderOffset;
        target.position += orientationVector(target.orientation) * wanderRadius;
        Vector3 steering = target.position - agent.position;
        return steering.normalized * maxAcceleration;
    }

}
