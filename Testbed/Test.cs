﻿using Box2D;
using Silk.NET.Input;
using Testbed.Drawing;

namespace Testbed;

internal class TestDestructionListener : DestructionListener
{
    private readonly Test _test;

    public TestDestructionListener(Test test)
    {
        _test = test;
    }

    protected override void SayGoodbye(Joint joint)
    {
        if (_test.MouseJoint == joint)
        {
            _test.MouseJoint = null;
        }
        else
        {
            _test.JointDestroyed(joint);
        }
    }
}

internal class TestQueryCallback : QueryCallback
{
    public Vec2 Point { get; private set; }

    public Fixture? Fixture { get; private set; }

    public void Reset(Vec2 point)
    {
        Point = point;
        Fixture = null;
    }

    protected override bool ReportFixture(Fixture fixture)
    {
        var body = fixture.Body;
        if (body.Type == BodyType.Dynamic)
        {
            var inside = fixture.TestPoint(Point);
            if (inside)
            {
                Fixture = fixture;
                return false;
            }
        }

        return true;
    }
}

struct ContactPoint
{
    public Fixture? FixtureA { get; init; }

    public Fixture? FixtureB { get; init; }

    public Vec2 Normal { get; init; }

    public Vec2 Position { get; init; }

    public PointState State { get; init; }

    public float NormalImpulse { get; init; }

    public float TangentImpulse { get; init; }

    public float Separation { get; set; }
}

internal class Test : ContactListener
{
    protected const int TextIncrement = 13;

    protected const int MaxContactPoints = 2048;

    private readonly WorldManifold _worldManifold = new();

    private readonly TestQueryCallback _queryCallback = new();

    private readonly MouseJointDef _mouseJointDef = new();

    private readonly CircleShape _bombShape = new() { Radius = 0.3f };

    protected DebugDraw DebugDraw { get; private set; } = default!;

    protected Settings Settings { get; private set; } = default!;

    protected Camera Camera { get; private set; } = default!;

    public MouseJoint? MouseJoint { get; set; } = default!;

    protected World World { get; }

    protected TestDestructionListener DestructionListener { get; }

    protected ContactPoint[] Points { get; } = new ContactPoint[MaxContactPoints];

    protected Body GroundBody { get; }

    protected Body? Bomb { get; private set; } = default!;

    protected int PointCount { get; private set; }

    protected Vec2 BombSpawnPoint { get; private set; }

    protected bool BombSpawning { get; private set; }

    protected Vec2 MouseWorld { get; private set; }

    protected int StepCount { get; private set; }

    protected int TextLine { get; set; } = 30;

    protected Profile MaxProfile { get; private set; }

    protected Profile TotalProfile { get; private set; }

    public Test()
    {
        DestructionListener = new(this);
        World = new World(new(0f, -10f));
        World.SetDestructionListener(DestructionListener);
        World.SetContactListener(this);

        using var bodyDef = new BodyDef();
        GroundBody = World.CreateBody(bodyDef);
    }

    public void Initialize(DebugDraw debugDraw, Settings settings, Camera camera)
    {
        DebugDraw = debugDraw;
        Settings = settings;
        Camera = camera;

        World.SetDebugDraw(DebugDraw);
    }

    public virtual void Step()
    {
        var timeStep = Settings.hertz > 0f ? 1f / Settings.hertz : 0f;

        if (Settings.pause)
        {
            if (Settings.singleStep)
            {
                Settings.singleStep = false;
            }
            else
            {
                timeStep = 0f;
            }

            DebugDraw.DrawString(5, TextLine, "****PAUSED****");
            TextLine += TextIncrement;
        }

        uint flags = 0;

        if (Settings.drawShapes)
        {
            flags += (uint)DrawFlags.ShapeBit;
        }

        if (Settings.drawJoints)
        {
            flags += (uint)DrawFlags.JointBit;
        }

        if (Settings.drawAABBs)
        {
            flags += (uint)DrawFlags.AabbBit;
        }

        if (Settings.drawCOMs)
        {
            flags += (uint)DrawFlags.CenterOfMassBit;
        }

        DebugDraw.Flags = (DrawFlags)flags;

        World.AllowSleeping = Settings.enableSleep;
        World.WarmStarting = Settings.enableWarmStarting;
        World.ContinuousPhysics = Settings.enableContinuous;
        World.SubStepping = Settings.enableSubStepping;

        PointCount = 0;

        World.Step(timeStep, Settings.velocityIterations, Settings.positionIterations);
        World.DebugDraw();

        DebugDraw.Flush();

        if (timeStep > 0f)
        {
            StepCount++;
        }

        if (Settings.drawStats)
        {
            var bodyCount = World.BodyCount;
            var contactCount = World.ContactCount;
            var jointCount = World.JointCount;
            DebugDraw.DrawString(5, TextLine, $"bodies/contacts/joints = {bodyCount}/{contactCount}/{jointCount}");
            TextLine += TextIncrement;

            var proxyCount = World.ProxyCount;
            var height = World.TreeHeight;
            var balance = World.TreeBalance;
            var quality = World.TreeQuality;
            DebugDraw.DrawString(5, TextLine, $"proxies/height/balance/quality = {proxyCount}/{height}/{balance}/{quality}");
            TextLine += TextIncrement;

            if (Box2DObjectTracker.Instance is { } tracker)
            {
                DebugDraw.DrawString(5, TextLine, $"managed objects = {tracker.ObjectCount}");
                TextLine += TextIncrement;
            }
        }

        World.GetProfile(out var profile);

        MaxProfile = new()
        {
            Step = Math.Max(MaxProfile.Step, profile.Step),
            Collide = Math.Max(MaxProfile.Collide, profile.Collide),
            Solve = Math.Max(MaxProfile.Solve, profile.Solve),
            SolveInit = Math.Max(MaxProfile.SolveInit, profile.SolveInit),
            SolveVelocity = Math.Max(MaxProfile.SolveVelocity, profile.SolveVelocity),
            SolvePosition = Math.Max(MaxProfile.SolvePosition, profile.SolvePosition),
            SolveTOI = Math.Max(MaxProfile.SolveTOI, profile.SolveTOI),
            Broadphase = Math.Max(MaxProfile.Broadphase, profile.Broadphase),
        };

        TotalProfile = new()
        {
            Step = TotalProfile.Step + profile.Step,
            Collide = TotalProfile.Collide + profile.Collide,
            Solve = TotalProfile.Solve + profile.Solve,
            SolveInit = TotalProfile.SolveInit + profile.SolveInit,
            SolveVelocity = TotalProfile.SolveVelocity + profile.SolveVelocity,
            SolvePosition = TotalProfile.SolvePosition + profile.SolvePosition,
            SolveTOI = TotalProfile.SolveTOI + profile.SolveTOI,
            Broadphase = TotalProfile.Broadphase + profile.Broadphase,
        };

        if (Settings.drawProfile)
        {
            var averageProfile = new Profile();

            if (StepCount > 0)
            {
                var scale = 1f / StepCount;
                averageProfile.Step = scale * TotalProfile.Step;
                averageProfile.Collide = scale * TotalProfile.Collide;
                averageProfile.Solve = scale * TotalProfile.Solve;
                averageProfile.SolveInit = scale * TotalProfile.SolveInit;
                averageProfile.SolveVelocity = scale * TotalProfile.SolveVelocity;
                averageProfile.SolvePosition = scale * TotalProfile.SolvePosition;
                averageProfile.SolveTOI = scale * TotalProfile.SolveTOI;
                averageProfile.Broadphase = scale * TotalProfile.Broadphase;
            }

            DebugDraw.DrawString(5, TextLine, $"step [ave] (max) = {profile.Step:.00} [{averageProfile.Step:.00}] ({MaxProfile.Step:.00})");
            TextLine += TextIncrement;
            DebugDraw.DrawString(5, TextLine, $"collide [ave] (max) = {profile.Collide:.00} [{averageProfile.Collide:.00}] ({MaxProfile.Collide:.00})");
            TextLine += TextIncrement;
            DebugDraw.DrawString(5, TextLine, $"solve [ave] (max) = {profile.Solve:.00} [{averageProfile.Solve:.00}] ({MaxProfile.Solve:.00})");
            TextLine += TextIncrement;
            DebugDraw.DrawString(5, TextLine, $"solve init [ave] (max) = {profile.SolveInit:.00} [{averageProfile.SolveInit:.00}] ({MaxProfile.SolveInit:.00})");
            TextLine += TextIncrement;
            DebugDraw.DrawString(5, TextLine, $"solve velocity [ave] (max) = {profile.SolveVelocity:.00} [{averageProfile.SolveVelocity:.00}] ({MaxProfile.SolveVelocity:.00})");
            TextLine += TextIncrement;
            DebugDraw.DrawString(5, TextLine, $"solve position [ave] (max) = {profile.SolvePosition:.00} [{averageProfile.SolvePosition:.00}] ({MaxProfile.SolvePosition:.00})");
            TextLine += TextIncrement;
            DebugDraw.DrawString(5, TextLine, $"solveTOI [ave] (max) = {profile.SolveTOI:.00} [{averageProfile.SolveTOI:.00}] ({MaxProfile.SolveTOI:.00})");
            TextLine += TextIncrement;
            DebugDraw.DrawString(5, TextLine, $"broad-phase [ave] (max) = {profile.Broadphase:.00} [{averageProfile.Broadphase:.00}] ({MaxProfile.Broadphase:.00})");
            TextLine += TextIncrement;
        }

        if (BombSpawning)
        {
            DebugDraw.DrawPoint(BombSpawnPoint, 4f, new Color(0f, 0f, 1f));
            DebugDraw.DrawSegment(MouseWorld, BombSpawnPoint, new Color(0.8f, 0.8f, 0.8f));
        }

        if (Settings.drawContactPoints)
        {
            var impulseScale = 0.1f;
            var axisScale = 0.3f;

            for (var i = 0; i < PointCount; i++)
            {
                ref var point = ref Points[i];

                if (point.State == PointState.Add)
                {
                    DebugDraw.DrawPoint(point.Position, 10f, new(0.3f, 0.95f, 0.3f));
                }
                else if (point.State == PointState.Persist)
                {
                    DebugDraw.DrawPoint(point.Position, 5f, new(0.3f, 0.3f, 0.95f));
                }

                if (Settings.drawContactNormals)
                {
                    var p1 = point.Position;
                    var p2 = p1 + axisScale * point.Normal;
                    DebugDraw.DrawSegment(p1, p2, new(0.9f, 0.9f, 0.9f));
                }
                else if (Settings.drawContactImpulse)
                {
                    var p1 = point.Position;
                    var p2 = p1 + impulseScale * point.NormalImpulse * point.Normal;
                    DebugDraw.DrawSegment(p1, p2, new(0.9f, 0.9f, 0.3f));
                }

                if (Settings.drawFrictionImpulse)
                {
                    var tangent = point.Normal.Cross(1f);
                    var p1 = point.Position;
                    var p2 = p1 + impulseScale * point.TangentImpulse * tangent;
                    DebugDraw.DrawSegment(p1, p2, new(0.9f, 0.9f, 0.3f));
                }
            }
        }
    }

    public virtual void UpdateUI()
    {
    }

    public virtual void JointDestroyed(Joint joint)
    {
    }

    public void ShiftOrigin(Vec2 newOrigin)
    {
        World.ShiftOrigin(newOrigin);
    }

    public void DrawTitle(string title)
    {
        DebugDraw.DrawString(5, 5, title);
        TextLine = 26;
    }

    protected override void PreSolve(in Contact contact, in Manifold oldManifold)
    {
        var manifold = contact.Manifold;

        if (manifold.Points.Length == 0)
        {
            return;
        }

        var fixtureA = contact.FixtureA;
        var fixtureB = contact.FixtureB;

        Span<PointState> state1 = stackalloc PointState[2];
        Span<PointState> state2 = stackalloc PointState[2];

        Collision.GetPointStates(state1, state2, oldManifold, manifold);

        contact.GetWorldManifold(_worldManifold);

        var points = _worldManifold.Points;

        for (var i = 0; i < manifold.Points.Length && PointCount < MaxContactPoints; i++, PointCount++)
        {
            Points[PointCount] = new()
            {
                FixtureA = fixtureA,
                FixtureB = fixtureB,
                Position = _worldManifold.Points[i],
                Normal = _worldManifold.Normal,
                State = state2[i],
                NormalImpulse = manifold.Points[i].NormalImpulse,
                TangentImpulse = manifold.Points[i].TangentImpulse,
                Separation = _worldManifold.Separations[i],
            };
        }
    }

    public virtual void Keyboard(Key key)
    {
    }

    public virtual void KeyboardUp(Key key)
    {
    }

    public virtual void MouseDown(Vec2 p)
    {
        MouseWorld = p;

        if (MouseJoint is not null)
        {
            return;
        }

        var aabb = new AABB();
        var d = new Vec2(0.001f, 0.001f);
        aabb.LowerBound = p - d;
        aabb.UpperBound = p + d;

        _queryCallback.Reset(p);
        World.QueryAABB(_queryCallback, aabb);

        if (_queryCallback.Fixture is not null)
        {
            var frequencyHz = 5f;
            var dampingRatio = 0.7f;

            var body = _queryCallback.Fixture.Body;
            _mouseJointDef.BodyA = GroundBody;
            _mouseJointDef.BodyB = body;
            _mouseJointDef.Target = p;
            _mouseJointDef.MaxForce = 1000f * body.Mass;
            Joint.LinearStiffness(out var stiffness, out var damping, frequencyHz, dampingRatio, _mouseJointDef.BodyA, _mouseJointDef.BodyB);
            _mouseJointDef.Stiffness = stiffness;
            _mouseJointDef.Damping = damping;

            MouseJoint = (MouseJoint)World.CreateJoint(_mouseJointDef);
            body.Awake = true;
        }
    }

    public virtual void ShiftMouseDown(Vec2 p)
    {
        MouseWorld = p;

        if (MouseJoint is not null)
        {
            return;
        }

        SpawnBomb(p);
    }

    public virtual void MouseUp(Vec2 p)
    {
        if (MouseJoint is not null)
        {
            World.DestroyJoint(MouseJoint);
            MouseJoint = null;
        }

        if (BombSpawning)
        {
            CompleteBombSpawn(p);
        }
    }

    public virtual void MouseMove(Vec2 p)
    {
        MouseWorld = p;

        if (MouseJoint is not null)
        {
            MouseJoint.Target = p;
        }
    }

    private void SpawnBomb(Vec2 worldPoint)
    {
        BombSpawnPoint = worldPoint;
        BombSpawning = true;
    }

    private void CompleteBombSpawn(Vec2 p)
    {
        if (!BombSpawning)
        {
            return;
        }

        var multiplier = 30f;
        var vel = BombSpawnPoint - p;
        vel *= multiplier;
        LaunchBomb(BombSpawnPoint, vel);
        BombSpawning = false;
    }

    public void LaunchBomb()
    {
        var p = new Vec2(MathUtils.RandomFloat(-15f, 15f), 30f);
        var v = -5f * p;
        LaunchBomb(p, v);
    }

    public void LaunchBomb(Vec2 position, Vec2 velocity)
    {
        if (Bomb is not null)
        {
            World.DestroyBody(Bomb);
            Bomb = null;
        }

        using var bd = new BodyDef
        {
            Type = BodyType.Dynamic,
            Position = position,
            Bullet = true,
        };
        Bomb = World.CreateBody(bd);
        Bomb.LinearVelocity = velocity;

        using var fd = new FixtureDef
        {
            Shape = _bombShape,
            Density = 20f,
            Restitution = 0f,
        };

        Bomb.CreateFixture(fd);
    }

    protected override void Dispose(bool disposing)
    {
        World.Dispose();
        _worldManifold.Dispose();
        _queryCallback.Dispose();
        _mouseJointDef.Dispose();
        _bombShape.Dispose();

        base.Dispose(disposing);
    }
}
