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
    public float wanderRadius = 100;
    public float wanderRate = 20;
    private float wanderOrientation;
    public Vector3 wanderCircleCenter;

    // Holds the path to follow
    public GameObject[] Path;
    public int current = 0;

    protected void Start() {
        agent = GetComponent<NPCController>();
        if(agent.gameObject.name == "Player") {
            Debug.Log("got it");
        }
        //wanderOrientation = agent.orientation;
    }

    public Vector3 Seek() {
        Vector3 direction = target.position - agent.position;
        direction.Normalize();
        direction *= maxAcceleration;
        return direction;
    }


    public Vector3 Flee() {
        Vector3 direction = agent.position - target.position;
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
    
    
    public Vector3 Arrive() {

        // Create the structure to hold our output
        Vector3 steering;

        // get the direction to the target 
        Vector3 direction = target.position - agent.position;
        float distance = direction.magnitude;
        float targetSpeed;
        // Check if we are there, return no steering

        //  If we are outside the slowRadius, then go max speed
        if (distance < targetRadiusL) {
            //return Vector3.zero;
            targetSpeed = 0;
        }
        else if (distance > slowRadiusL) {
            targetSpeed = maxSpeed;
        } // Otherwise calculate a scaled speed
        else {
            targetSpeed = (maxSpeed * distance) / slowRadiusL;
        }

       // The target velocity combines speed and direction
       Vector3 targetVelocity = direction;
        targetVelocity.Normalize();
        targetVelocity *= targetSpeed;

        // Acceleration tries to get to the target velocity
        steering = targetVelocity - agent.velocity;
        steering = steering / timeToTarget;

        // Check if the acceleration is too fast
        if (steering.magnitude > maxAcceleration) {
            steering.Normalize();
            steering *= maxAcceleration; 
        }

        // output the steering 
        return steering;
    }
    
    
    public float Align() {

        // Create the structure to hold our output
        float steering_angular;

        // Get the naive direction to the target
        float rotation = target.orientation - agent.orientation;

        // map the result to the (-pi,pi) interval 
        while (rotation > Mathf.PI) {
            rotation -= 2 * Mathf.PI;
        }
        while (rotation < -Mathf.PI) {
            rotation += 2 * Mathf.PI;
        }
        float rotationSize = Mathf.Abs(rotation);

        // Check if we are there, return no steering
        if(rotationSize < targetRadiusA) {
            agent.rotation = 0;
        }

        // Otherwise calculate a scaled rotation 
        float targetRotation;
        if(rotationSize > slowRadiusA) {
            targetRotation = maxRotation;
        } else {
            targetRotation = (maxRotation * rotationSize) / slowRadiusA;
        }

       // The final target rotation combines
        // speed (already in the variable) and direction
        targetRotation *= (rotation / rotationSize);

        // Acceleration tries to get to the target rotation
        steering_angular = targetRotation - agent.rotation;
        steering_angular = steering_angular / timeToTarget;

        // Check if the acceleration is too great
        float angularAcceleration;
        angularAcceleration = Mathf.Abs(steering_angular);
        if(angularAcceleration > maxAngularAcceleration) {
            steering_angular = steering_angular / angularAcceleration;
            steering_angular = steering_angular / angularAcceleration;
            steering_angular *= maxAngularAcceleration;
        }
        // 
        // output the steering 
        return steering_angular;

    }
    

    private Vector3 orientationVector(float angle) {
        return new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
    }


    public Vector3 Wander(out float angular) {
        wanderOrientation += (Random.value - Random.value) * wanderRate;
        float target_orientation = agent.orientation + wanderOrientation;
        Vector3 target_position = agent.position + orientationVector(agent.orientation) * wanderOffset;
        wanderCircleCenter = target_position;
        target_position += orientationVector(target_orientation) * wanderRadius;
        Vector3 steering = target_position - agent.position;
        if(target == null) {
            target = new NPCController();
        }

        target.orientation = Mathf.Atan2(steering.x, steering.z);
        angular = Align();
        return steering.normalized * maxAcceleration;
    }

}
