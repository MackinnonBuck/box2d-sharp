﻿using Box2D;
using System;
using Xunit;

namespace UnitTests;

public class JointTests
{
    [Fact]
    public void JointTest_Works()
    {
        var gravity = new Vec2(0f, -10f);
        using var world = new World(gravity);

        var bodyDef = new BodyDef
        {
            Type = BodyType.Dynamic,
            Position = new(-2f, 3f),
        };
        var ground = world.CreateBody(bodyDef);

        using var circle = new CircleShape
        {
            Radius = 1f,
        };

        var fixtureDef = new FixtureDef
        {
            Filter = new()
            {
                MaskBits = 0,
            },
            Density = 1f,
            Shape = circle,
        };

        var bodyA = world.CreateBody(bodyDef);
        var bodyB = world.CreateBody(bodyDef);
        var bodyC = world.CreateBody(bodyDef);

        circle.ComputeMass(out var massData, fixtureDef.Density);
        var mg = massData.Mass * gravity.Y;

        bodyA.CreateFixture(fixtureDef);
        bodyB.CreateFixture(fixtureDef);
        bodyC.CreateFixture(fixtureDef);

        using var distanceJointDef = new DistanceJointDef();
        distanceJointDef.Initialize(ground, bodyA, bodyDef.Position + new Vec2(0f, 4f), bodyDef.Position);
        distanceJointDef.MinLength = distanceJointDef.Length;
        distanceJointDef.MaxLength = distanceJointDef.Length;

        using var prismaticJointDef = new PrismaticJointDef();
        prismaticJointDef.Initialize(ground, bodyB, bodyDef.Position, new Vec2(1f, 0f));

        using var revoluteJointDef = new RevoluteJointDef();
        revoluteJointDef.Initialize(ground, bodyC, bodyDef.Position);

        var distanceJoint = (DistanceJoint)world.CreateJoint(distanceJointDef);
        var prismaticJoint = (PrismaticJoint)world.CreateJoint(prismaticJointDef);
        var revoluteJoint = (RevoluteJoint)world.CreateJoint(revoluteJointDef);

        var timeStep = 1f / 60f;
        var invTimeStep = 60f;
        var velocityIterations = 6;
        var positionIterations = 2;

        world.Step(timeStep, velocityIterations, positionIterations);

        var tol = 1e-5f;

        {
            var f = distanceJoint.GetReactionForce(invTimeStep);
            var t = distanceJoint.GetReactionTorque(invTimeStep);
            Assert.Equal(0f, f.X);
            Assert.True(MathF.Abs(f.Y + mg) < tol);
            Assert.Equal(0f, t);
        }

        {
            var f = prismaticJoint.GetReactionForce(invTimeStep);
            var t = prismaticJoint.GetReactionTorque(invTimeStep);
            Assert.Equal(0f, f.X);
            Assert.True(MathF.Abs(f.Y + mg) < tol);
            Assert.Equal(0f, t);
        }

        {
            var f = revoluteJoint.GetReactionForce(invTimeStep);
            var t = revoluteJoint.GetReactionTorque(invTimeStep);
            Assert.Equal(0f, f.X);
            Assert.True(MathF.Abs(f.Y + mg) < tol);
            Assert.Equal(0f, t);
        }
    }
}
