#include "Vector2.h"
#include "RVOSimulator.h"
#include "Agent.h"
#include <map>

using namespace RVO;

#if _MSC_VER
#define EXPORT_API __declspec (dllexport)
#else
#define EXPORT_API // Xcode does not need
#endif

typedef struct
{
	float x;
	float y;
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

	EXPORT_API size_t AddAgent(int id,  float posX, float posY, float neighborDist, size_t maxNeighbors, float timeHorizon, float timeHorizonObst,
		float radius, float maxSpeed, float mass, float velocityX, float velocityY)
	{
		Vector2 position = Vector2(posX, posY);
		Vector2 velocity = Vector2(velocityX, velocityY);
        
		return pRVOSimulators[id]->addAgent(position, neighborDist, maxNeighbors, timeHorizon, timeHorizonObst, radius, maxSpeed, mass, MonsterType::NormalGround, velocity);
	}

	EXPORT_API void RemoveAgent(int id, size_t agentIndex)
	{
		pRVOSimulators[id]->removeAgent(agentIndex);
	}

	EXPORT_API void GetAgentPosition(int id, size_t agentIndex, AgentPosition& position)
	{
		Vector2 vec = pRVOSimulators[id]->getAgentPosition(agentIndex);
		position.x = vec.x();
		position.y = vec.y();
	}

	EXPORT_API void SetTimeStep(int id, float timeStep)
	{
		pRVOSimulators[id]->setTimeStep(timeStep);
	}

	EXPORT_API void SetAgentVelocityPref(int id, size_t agentIndex, float x, float y)
	{
		pRVOSimulators[id]->setAgentPrefVelocity(agentIndex, Vector2(x, y));
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

	EXPORT_API int GetNearByAgents(int id, float x, float y, void* intArray, int arraySize, float range)
	{
		int *ptr = (int *)intArray;
		Vector2 pos = Vector2(x, y);
		int findCount = 0;

		findCount = pRVOSimulators[id]->QueryNearByAgents(pos, ptr, findCount, arraySize, range, 0);

		return findCount;
	}
}
