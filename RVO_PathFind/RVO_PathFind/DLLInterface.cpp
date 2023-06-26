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

#define FLY_AGENT

typedef struct
{
	float x;
	float y;
}AgentPosition;

extern "C"
{
#ifdef FLY_AGENT
	std::map<AgentType, RVOSimulator*> pRVOSimulators;
#else
	RVOSimulator* pRVOSimulator;
#endif

AgentType CastAgentType(size_t type);

	//EXPORT_API void InitSystem(float timeStep, float neighborDist, size_t maxNeighbors, float timeHorizon, 
	//	float timeHorizonObst, float radius, float maxSpeed)
	//{
	//	pRVOSimulator = new RVOSimulator(timeStep, neighborDist, maxNeighbors, timeHorizon, timeHorizonObst, radius, maxSpeed);
	//}

	EXPORT_API void InitSystem()
	{
#ifdef FLY_AGENT
		pRVOSimulators[AgentType::Ground] = new RVOSimulator();
		pRVOSimulators[AgentType::Fly] = new RVOSimulator();
#else
		pRVOSimulator = new RVOSimulator();
#endif
	}

	EXPORT_API void ProcessObstacles()
	{
#ifdef FLY_AGENT
		pRVOSimulators[AgentType::Ground]->processObstacles();
#else
		pRVOSimulator->processObstacles();
#endif
	}


	EXPORT_API void Shutdown()
	{
#ifdef FLY_AGENT
		for (std::map<AgentType, RVOSimulator*>::iterator iter = pRVOSimulators.begin(); iter != pRVOSimulators.end(); ++ iter)
		{
			delete iter->second;
			iter->second = NULL;
		}
		pRVOSimulators.clear();
#else
		if (pRVOSimulator != NULL)
		{
			delete pRVOSimulator;
			pRVOSimulator = NULL;
		}
#endif
	}

	EXPORT_API size_t AddAgent(size_t type,  float posX, float posY, float neighborDist, size_t maxNeighbors, float timeHorizon, float timeHorizonObst,
		float radius, float maxSpeed, float mass, float velocityX, float velocityY)
	{
		Vector2 position = Vector2(posX, posY);
		Vector2 velocity = Vector2(velocityX, velocityY);
        
        MonsterType _MonsterType = (MonsterType)type;

#ifdef FLY_AGENT
        AgentType agentType = CastAgentType(type);
		return pRVOSimulators[agentType]->addAgent(position, neighborDist, maxNeighbors, timeHorizon, timeHorizonObst, radius, maxSpeed, mass, _MonsterType, velocity);
#else
		return pRVOSimulator->addAgent(position, neighborDist, maxNeighbors, timeHorizon, timeHorizonObst, radius, maxSpeed, mass, velocity);
#endif
	}

	EXPORT_API void RemoveAgent(size_t type, size_t agentIndex)
	{
#ifdef FLY_AGENT
        AgentType agentType = CastAgentType(type);
		pRVOSimulators[agentType]->removeAgent(agentIndex);
#else
		pRVOSimulator->removeAgent(agentIndex);
#endif
	}

	//EXPORT_API size_t AddObstacle(const std::vector<Vector2>& vertices)
	//{
	//	return pRVOSimulator->addObstacle(vertices);
	//}

	EXPORT_API size_t AddObstacle(float* points, size_t count)
	{
		if (count <= 0)
			return -1;

		std::vector<Vector2> vertices;
		for (size_t i = 0; i < count;)
		{
			vertices.push_back(Vector2(points[i], points[i + 1]));
			i += 2;
		}

#ifdef FLY_AGENT
		return pRVOSimulators[AgentType::Ground]->addObstacle(vertices);
#else
		return pRVOSimulator->addObstacle(vertices);
#endif
	}

    EXPORT_API void ClearObstacle()
    {
#ifdef FLY_AGENT
        return pRVOSimulators[AgentType::Ground]->ClearObstacle();
#else
        return pRVOSimulator->ClearObstacle();
#endif
    }

	EXPORT_API void SetAgentMaxSpeed(size_t type, size_t agentIndex, float maxSpeed)
	{
#ifdef FLY_AGENT
        AgentType agentType = CastAgentType(type);
		pRVOSimulators[agentType]->setAgentMaxSpeed(agentIndex, maxSpeed);
#else
		pRVOSimulator->setAgentMaxSpeed(agentIndex, maxSpeed);
#endif
	}

	EXPORT_API float GetAgentMaxSpeed(size_t type, size_t agentIndex)
	{
#ifdef FLY_AGENT
        AgentType agentType = CastAgentType(type);
		return pRVOSimulators[agentType]->getAgentMaxSpeed(agentType);
#else
		return pRVOSimulator->getAgentMaxSpeed(agentIndex);
#endif
	}

	EXPORT_API void SetAgentRadius(size_t type, size_t agentIndex, float radius)
	{
#ifdef FLY_AGENT
        AgentType agentType = CastAgentType(type);
		pRVOSimulators[agentType]->setAgentRadius(agentIndex, radius);
#else
		pRVOSimulator->setAgentRadius(agentIndex, radius);
#endif
	}

	EXPORT_API float GetAgentRadius(size_t type, size_t agentIndex)
	{
#ifdef FLY_AGENT
        AgentType agentType = CastAgentType(type);
		return pRVOSimulators[agentType]->getAgentRadius(agentIndex);
#else
		return pRVOSimulator->getAgentRadius(agentIndex);
#endif
	}

	EXPORT_API void GetAgentPosition(size_t type, size_t agentIndex, AgentPosition& position)
	{
#ifdef FLY_AGENT
        AgentType agentType = CastAgentType(type);
		Vector2 vec = pRVOSimulators[agentType]->getAgentPosition(agentIndex);
#else
		Vector2 vec = pRVOSimulator->getAgentPosition(agentIndex);
#endif
		position.x = vec.x();
		position.y = vec.y();
	}

	EXPORT_API void SetAgentPosition(size_t type, size_t agentIndex, float x, float y)
	{
#ifdef FLY_AGENT
        AgentType agentType = CastAgentType(type);
		pRVOSimulators[agentType]->setAgentPosition(agentIndex, Vector2(x, y));
#else
		pRVOSimulator->setAgentPosition(agentIndex, Vector2(x, y));
#endif
	}

	EXPORT_API void SetTimeStep(float timeStep)
	{
#ifdef FLY_AGENT
		for (std::map<AgentType, RVOSimulator*>::iterator iter = pRVOSimulators.begin(); iter != pRVOSimulators.end(); ++ iter)
		{
			iter->second->setTimeStep(timeStep);
		}
#else
		pRVOSimulator->setTimeStep(timeStep);
#endif
	}

	EXPORT_API size_t GetNumAgents(size_t type)
	{
#ifdef FLY_AGENT
        AgentType agentType = CastAgentType(type);
		return pRVOSimulators[agentType]->getNumAgents();
#else
		return pRVOSimulator->getNumAgents();
#endif
	}

	EXPORT_API void SetAgentVelocity(size_t type, size_t agentIndex, float x, float y)
	{
#ifdef FLY_AGENT
        AgentType agentType = CastAgentType(type);
		pRVOSimulators[agentType]->setAgentVelocity(agentIndex, Vector2(x, y));
#else
		pRVOSimulator->setAgentVelocity(agentIndex, Vector2(x, y));
#endif
	}

	EXPORT_API void SetAgentVelocityPref(size_t type, size_t agentIndex, float x, float y)
	{
#ifdef FLY_AGENT
        AgentType agentType = CastAgentType(type);
		pRVOSimulators[agentType]->setAgentPrefVelocity(agentIndex, Vector2(x, y));
#else
		pRVOSimulator->setAgentPrefVelocity(agentIndex, Vector2(x, y));
#endif
	}

	//EXPORT_API void SetAgentVelocityPref(size_t agentIndex, const Vector2& velocityPref)
	//{
	//	pRVOSimulator->setAgentPrefVelocity(agentIndex, velocityPref);
	//}

	EXPORT_API void SetAgentMass(size_t type, size_t agentIndex, float mass)
	{
#ifdef FLY_AGENT
		AgentType agentType = CastAgentType(type);
		pRVOSimulators[agentType]->setAgentMass(agentIndex, mass);
#else
		pRVOSimulator->setAgentMass(agentIndex, mass);
#endif
	}

	EXPORT_API void DoStep()
	{
#ifdef FLY_AGENT
		for (std::map<AgentType, RVOSimulator*>::iterator iter = pRVOSimulators.begin(); iter != pRVOSimulators.end(); ++ iter)
		{ 
			iter->second->doStep();
		}
#else
		pRVOSimulator->doStep();
#endif
	}

	EXPORT_API void DoStepBuildTree()
	{
#ifdef FLY_AGENT
		for (std::map<AgentType, RVOSimulator*>::iterator iter = pRVOSimulators.begin(); iter != pRVOSimulators.end(); ++ iter)
		{ 
			iter->second->doStepBuildTree();
		}
#else
		pRVOSimulator->doStepBuildTree();
#endif
	}

	EXPORT_API void doStepNeighborAndVelocity(int index, int max)
	{
#ifdef FLY_AGENT
		for (std::map<AgentType, RVOSimulator*>::iterator iter = pRVOSimulators.begin(); iter != pRVOSimulators.end(); ++ iter)
		{ 
			iter->second->doStepNeighborAndVelocity(index, max);
		}
#else
		pRVOSimulator->doStepNeighborAndVelocity(index, max);
#endif
	}

	EXPORT_API void DoStepUpdate()
	{
#ifdef FLY_AGENT
		for (std::map<AgentType, RVOSimulator*>::iterator iter = pRVOSimulators.begin(); iter != pRVOSimulators.end(); ++ iter)
		{ 
			iter->second->doStepUpdate();
		}
#else
		pRVOSimulator->doStepUpdate();
#endif
	}


    AgentType CastAgentType(size_t type)
    {
		if (type == 1 || type == 3)
			return AgentType::Fly;

		return AgentType::Ground;
    }
	
	EXPORT_API int GetNearByAgents(float x, float y, void* intArray, int arraySize, float range)
	{
		int *ptr = (int *)intArray;
		Vector2 pos = Vector2(x, y);
		int findCount = 0;
#ifdef FLY_AGENT
		if (pRVOSimulators.size() < 2) return 0;

		findCount = pRVOSimulators[AgentType::Ground]->QueryNearByAgents(pos, ptr, findCount, arraySize, range, 0);
		findCount = pRVOSimulators[AgentType::Fly]->QueryNearByAgents(pos, ptr, findCount, arraySize, range, 100000);
#else
		if (pRVOSimulators.size() < 1) return 0;

		pRVOSimulator->QueryNearByAgents(pos, ptr, arraySize, range);
#endif

		return findCount;
	}
}
