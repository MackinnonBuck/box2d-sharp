#pragma once

#include "api.h"

#include <box2d/b2_world.h>

extern "C"
{
    BOX2D_API b2World* b2World_new(b2Vec2* gravity);
    BOX2D_API b2Body* b2World_CreateBody(b2World* obj, b2BodyDef* def);
    BOX2D_API void b2World_DestroyBody(b2World* obj, b2Body* body);
    BOX2D_API b2Joint* b2World_CreateJoint(b2World* obj, b2JointDef* def);
    BOX2D_API void b2World_DestroyJoint(b2World* obj, b2Joint* joint);
    BOX2D_API void b2World_Step(b2World* obj, float timeStep, int32 velocityIterations, int32 positionIterations);
    BOX2D_API void b2World_ClearForces(b2World* obj);
    BOX2D_API b2Body* b2World_GetBodyList(b2World* obj);
    BOX2D_API b2Joint* b2World_GetJointList(b2World* obj);
    BOX2D_API void b2World_delete(b2World* obj);
}
