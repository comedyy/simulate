#include "Vector2.h"
#include "RVOSimulator.h"
#include "Agent.h"
#include <map>
#include "../libfixmath/fix16.hpp"

using namespace RVO;

#if _MSC_VER
#define EXPORT_API __declspec (dllexport)
#else
#define EXPORT_API // Xcode does not need
#endif

typedef struct
{
	fix16_t x;
	fix16_t y;
}AgentPosition;

extern "C"
{
	std::map<int, RVOSimulator*> pRVOSimulators;

	EXPORT_API void InitSystem(int id)
	{
		pRVOSimulators[id] = new RVOSimulator();
	}


	EXPORT_API void Shutdown(int id)
	{
		RVOSimulator* x = pRVOSimulators[id];
		delete x;
	}

	EXPORT_API size_t AddAgent(int id,  fix16_t posX, fix16_t posY, fix16_t neighborDist, size_t maxNeighbors, fix16_t timeHorizon, fix16_t timeHorizonObst,
		fix16_t radius, fix16_t maxSpeed, fix16_t mass, fix16_t velocityX, fix16_t velocityY)
	{
		Vector2 position = Vector2(Fix16::FromRaw(posX), Fix16::FromRaw(posY));
		Vector2 velocity = Vector2(Fix16::FromRaw(velocityX), Fix16::FromRaw(velocityY));
        
		return pRVOSimulators[id]->addAgent(position, Fix16::FromRaw(neighborDist), maxNeighbors, Fix16::FromRaw(timeHorizon), Fix16::FromRaw(timeHorizonObst), 
			Fix16::FromRaw(radius), Fix16::FromRaw(maxSpeed), Fix16::FromRaw(mass), MonsterType::NormalGround, velocity);
	}

	EXPORT_API void RemoveAgent(int id, size_t agentIndex)
	{
		pRVOSimulators[id]->removeAgent(agentIndex);
	}

	EXPORT_API void GetAgentPosition(int id, size_t agentIndex, AgentPosition& position)
	{
		Vector2 vec = pRVOSimulators[id]->getAgentPosition(agentIndex);
		position.x = vec.x().value;
		position.y = vec.y().value;
	}

	EXPORT_API void SetTimeStep(int id, fix16_t timeStep)
	{
		pRVOSimulators[id]->setTimeStep(Fix16::FromRaw(timeStep));
	}

	EXPORT_API void SetAgentVelocityPref(int id, size_t agentIndex, fix16_t x, fix16_t y)
	{
		pRVOSimulators[id]->setAgentPrefVelocity(agentIndex, Vector2(Fix16::FromRaw(x), Fix16::FromRaw(y)));
	}

	EXPORT_API void DoStep(int id)
	{
		pRVOSimulators[id]->doStep();
	}

	EXPORT_API void DoStepBuildTree(int id)
	{
		pRVOSimulators[id]->doStepBuildTree();
	}

	EXPORT_API void doStepNeighborAndVelocity(int id, int index, int max)
	{
		pRVOSimulators[id]->doStepNeighborAndVelocity(index, max);
	}

	EXPORT_API void DoStepUpdate(int id)
	{
		pRVOSimulators[id]->doStepUpdate();
	}

	EXPORT_API int GetNearByAgents(int id, fix16_t x, fix16_t y, void* intArray, int arraySize, fix16_t range)
	{
		int *ptr = (int *)intArray;
		Vector2 pos = Vector2(Fix16::FromRaw(x), Fix16::FromRaw(y));
		int findCount = 0;

		findCount = pRVOSimulators[id]->QueryNearByAgents(pos, ptr, findCount, arraySize, Fix16::FromRaw(range), 0);

		return findCount;
	}
}
