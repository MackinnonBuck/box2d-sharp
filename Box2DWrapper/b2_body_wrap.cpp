#include "pch.h"
#include "verify.h"
#include "b2_body_wrap.h"

b2Fixture* b2Body_CreateFixture(b2Body* obj, b2FixtureDef* def)
{
    VERIFY_INSTANCE;
    return obj->CreateFixture(def);
}

void b2Body_GetTransform(b2Body* obj, b2Transform* transform)
{
    VERIFY_INSTANCE;
    *transform = obj->GetTransform();
}

void b2Body_SetTransform(b2Body* obj, b2Vec2* position, float angle)
{
    VERIFY_INSTANCE;
    obj->SetTransform(*position, angle);
}

void b2Body_ApplyForce(b2Body* obj, b2Vec2* force, b2Vec2* point, bool wake)
{
    VERIFY_INSTANCE;
    obj->ApplyForce(*force, *point, wake);
}

void b2Body_ApplyForceToCenter(b2Body* obj, b2Vec2* force, bool wake)
{
    VERIFY_INSTANCE;
    obj->ApplyForceToCenter(*force, wake);
}

void b2Body_ApplyTorque(b2Body* obj, float torque, bool wake)
{
    VERIFY_INSTANCE;
    obj->ApplyTorque(torque, wake);
}

void b2Body_ApplyLinearImpulse(b2Body* obj, b2Vec2* impulse, b2Vec2* point, bool wake)
{
    VERIFY_INSTANCE;
    obj->ApplyLinearImpulse(*impulse, *point, wake);
}

void b2Body_ApplyLinearImpulseToCenter(b2Body* obj, b2Vec2* impulse, bool wake)
{
    VERIFY_INSTANCE;
    obj->ApplyLinearImpulseToCenter(*impulse, wake);
}

void b2Body_ApplyAngularImpulse(b2Body* obj, float impulse, bool wake)
{
    VERIFY_INSTANCE;
    obj->ApplyAngularImpulse(impulse, wake);
}

void b2Body_GetPosition(b2Body* obj, b2Vec2* v)
{
    VERIFY_INSTANCE;
    *v = obj->GetPosition();
}

float b2Body_GetAngle(b2Body* obj)
{
    VERIFY_INSTANCE;
    return obj->GetAngle();
}

void b2Body_GetLinearVelocity(b2Body* obj, b2Vec2* value)
{
    VERIFY_INSTANCE;
    *value = obj->GetLinearVelocity();
}

void b2Body_SetLinearVelocity(b2Body* obj, b2Vec2* v)
{
    VERIFY_INSTANCE;
    obj->SetLinearVelocity(*v);
}

float b2Body_GetAngularVelocity(b2Body* obj)
{
    VERIFY_INSTANCE;
    return obj->GetAngularVelocity();
}

void b2Body_SetAngularVelocity(b2Body* obj, float omega)
{
    VERIFY_INSTANCE;
    obj->SetAngularVelocity(omega);
}

float b2Body_GetMass(b2Body* obj)
{
    VERIFY_INSTANCE;
    return obj->GetMass();
}

bool b2Body_IsAwake(b2Body* obj)
{
    VERIFY_INSTANCE;
    return obj->IsAwake();
}

void b2Body_SetAwake(b2Body* obj, bool flag)
{
    VERIFY_INSTANCE;
    obj->SetAwake(flag);
}

b2Fixture* b2Body_GetFixtureList(b2Body* obj)
{
    VERIFY_INSTANCE;
    return obj->GetFixtureList();
}

b2JointEdge* b2Body_GetJointList(b2Body* obj)
{
    VERIFY_INSTANCE;
    return obj->GetJointList();
}

b2Body* b2Body_GetNext(b2Body* obj)
{
    VERIFY_INSTANCE;
    return obj->GetNext();
}

uintptr_t b2Body_GetUserData(b2Body* obj)
{
    VERIFY_INSTANCE;
    return obj->GetUserData().pointer;
}
