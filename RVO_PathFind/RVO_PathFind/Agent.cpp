#include "Agent.h"

#include "KdTree.h"
#include "Obstacle.h"
#include "../libfixmath/fix16.hpp"

namespace RVO {
	Agent::Agent(RVOSimulator *sim) : maxNeighbors_(0), maxSpeed_(Fix16::zero), neighborDist_(Fix16::zero), radius_(Fix16::zero), sim_(sim),
		timeHorizon_(Fix16::zero), timeHorizonObst_(Fix16::zero), id_(0), mass_(Fix16::one), m_IsRemove(false){ }

	void Agent::computeNeighbors()
	{
		obstacleNeighbors_.clear();
		Fix16 rangeSq = sqr(timeHorizonObst_ * maxSpeed_ + radius_);
		sim_->kdTree_->computeObstacleNeighbors(this, rangeSq);

		agentNeighbors_.clear();

		if (maxNeighbors_ > 0) {
			rangeSq = sqr(neighborDist_);
			sim_->kdTree_->computeAgentNeighbors(this, rangeSq);
		}
	}

	void Agent::computeNewVelocity()
	{
		orcaLines_.clear();

		const Fix16 invTimeHorizonObst = Fix16::one / timeHorizonObst_;

		for (size_t i = 0; i < obstacleNeighbors_.size(); ++i) {

			const Obstacle *obstacle1 = obstacleNeighbors_[i].second;
			const Obstacle *obstacle2 = obstacle1->nextObstacle_;

			const Vector2 relativePosition1 = obstacle1->point_ - position_;
			const Vector2 relativePosition2 = obstacle2->point_ - position_;

			bool alreadyCovered = false;

			for (size_t j = 0; j < orcaLines_.size(); ++j) {
				if (det(invTimeHorizonObst * relativePosition1 - orcaLines_[j].point, orcaLines_[j].direction) - invTimeHorizonObst * radius_ >= -Fix16::epsilon && det(invTimeHorizonObst * relativePosition2 - orcaLines_[j].point, orcaLines_[j].direction) - invTimeHorizonObst * radius_ >=  -Fix16::epsilon) {
					alreadyCovered = true;
					break;
				}
			}

			if (alreadyCovered) {
				continue;
			}

			const Fix16 distSq1 = absSq(relativePosition1);
			const Fix16 distSq2 = absSq(relativePosition2);

			const Fix16 radiusSq = sqr(radius_);

			const Vector2 obstacleVector = obstacle2->point_ - obstacle1->point_;
			const Fix16 s = (-relativePosition1 * obstacleVector) / absSq(obstacleVector);
			const Fix16 distSqLine = absSq(-relativePosition1 - s * obstacleVector);

			Line line;

			if (s < Fix16::zero && distSq1 <= radiusSq) {
				if (obstacle1->isConvex_) {
					line.point = Vector2(Fix16::zero, Fix16::zero);
					line.direction = normalize(Vector2(-relativePosition1.y(), relativePosition1.x()));
					orcaLines_.push_back(line);
				}

				continue;
			}
			else if (s > Fix16::one && distSq2 <= radiusSq) {
				if (obstacle2->isConvex_ && det(relativePosition2, obstacle2->unitDir_) >= Fix16::zero) {
					line.point = Vector2(Fix16::zero, Fix16::zero);
					line.direction = normalize(Vector2(-relativePosition2.y(), relativePosition2.x()));
					orcaLines_.push_back(line);
				}

				continue;
			}
			else if (s >= Fix16::zero && s <= Fix16::one && distSqLine <= radiusSq) {
				line.point = Vector2(Fix16::zero, Fix16::zero);
				line.direction = -obstacle1->unitDir_;
				orcaLines_.push_back(line);
				continue;
			}

			Vector2 leftLegDirection, rightLegDirection;

			if (s < Fix16::zero && distSqLine <= radiusSq) {
				if (!obstacle1->isConvex_) {
					continue;
				}

				obstacle2 = obstacle1;

				const Fix16 leg1 = (distSq1 - radiusSq).sqrt();
				leftLegDirection = Vector2(relativePosition1.x() * leg1 - relativePosition1.y() * radius_, relativePosition1.x() * radius_ + relativePosition1.y() * leg1) / distSq1;
				rightLegDirection = Vector2(relativePosition1.x() * leg1 + relativePosition1.y() * radius_, -relativePosition1.x() * radius_ + relativePosition1.y() * leg1) / distSq1;
			}
			else if (s > Fix16::one && distSqLine <= radiusSq) {
				if (!obstacle2->isConvex_) {
					continue;
				}

				obstacle1 = obstacle2;

				const Fix16 leg2 = (distSq2 - radiusSq).sqrt();
				leftLegDirection = Vector2(relativePosition2.x() * leg2 - relativePosition2.y() * radius_, relativePosition2.x() * radius_ + relativePosition2.y() * leg2) / distSq2;
				rightLegDirection = Vector2(relativePosition2.x() * leg2 + relativePosition2.y() * radius_, -relativePosition2.x() * radius_ + relativePosition2.y() * leg2) / distSq2;
			}
			else {
				if (obstacle1->isConvex_) {
					const Fix16 leg1 = (distSq1 - radiusSq).sqrt();
					leftLegDirection = Vector2(relativePosition1.x() * leg1 - relativePosition1.y() * radius_, relativePosition1.x() * radius_ + relativePosition1.y() * leg1) / distSq1;
				}
				else {
					leftLegDirection = -obstacle1->unitDir_;
				}

				if (obstacle2->isConvex_) {
					const Fix16 leg2 = (distSq2 - radiusSq).sqrt();
					rightLegDirection = Vector2(relativePosition2.x() * leg2 + relativePosition2.y() * radius_, -relativePosition2.x() * radius_ + relativePosition2.y() * leg2) / distSq2;
				}
				else {
					rightLegDirection = obstacle1->unitDir_;
				}
			}

			const Obstacle *const leftNeighbor = obstacle1->prevObstacle_;

			bool isLeftLegForeign = false;
			bool isRightLegForeign = false;

			if (obstacle1->isConvex_ && det(leftLegDirection, -leftNeighbor->unitDir_) >= Fix16::zero) {
				leftLegDirection = -leftNeighbor->unitDir_;
				isLeftLegForeign = true;
			}

			if (obstacle2->isConvex_ && det(rightLegDirection, obstacle2->unitDir_) <= Fix16::zero) {
				rightLegDirection = obstacle2->unitDir_;
				isRightLegForeign = true;
			}

			const Vector2 leftCutoff = invTimeHorizonObst * (obstacle1->point_ - position_);
			const Vector2 rightCutoff = invTimeHorizonObst * (obstacle2->point_ - position_);
			const Vector2 cutoffVec = rightCutoff - leftCutoff;

			const Fix16 t = (obstacle1 == obstacle2 ? Fix16::half : ((velocity_ - leftCutoff) * cutoffVec) / absSq(cutoffVec));
			const Fix16 tLeft = ((velocity_ - leftCutoff) * leftLegDirection);
			const Fix16 tRight = ((velocity_ - rightCutoff) * rightLegDirection);

			if ((t < Fix16::zero && tLeft < Fix16::zero) || (obstacle1 == obstacle2 && tLeft < Fix16::zero && tRight < Fix16::zero)) {
				const Vector2 unitW = normalize(velocity_ - leftCutoff);

				line.direction = Vector2(unitW.y(), -unitW.x());
				line.point = leftCutoff + radius_ * invTimeHorizonObst * unitW;
				orcaLines_.push_back(line);
				continue;
			}
			else if (t > Fix16::one && tRight < Fix16::zero) {
				const Vector2 unitW = normalize(velocity_ - rightCutoff);

				line.direction = Vector2(unitW.y(), -unitW.x());
				line.point = rightCutoff + radius_ * invTimeHorizonObst * unitW;
				orcaLines_.push_back(line);
				continue;
			}

			const Fix16 distSqCutoff = ((t < Fix16::zero || t > Fix16::one || obstacle1 == obstacle2) ? Fix16::infinity : absSq(velocity_ - (leftCutoff + t * cutoffVec)));
			const Fix16 distSqLeft = ((tLeft < Fix16::zero) ? Fix16::infinity : absSq(velocity_ - (leftCutoff + tLeft * leftLegDirection)));
			const Fix16 distSqRight = ((tRight < Fix16::zero) ? Fix16::infinity : absSq(velocity_ - (rightCutoff + tRight * rightLegDirection)));

			if (distSqCutoff <= distSqLeft && distSqCutoff <= distSqRight) {
				line.direction = -obstacle1->unitDir_;
				line.point = leftCutoff + radius_ * invTimeHorizonObst * Vector2(-line.direction.y(), line.direction.x());
				orcaLines_.push_back(line);
				continue;
			}
			else if (distSqLeft <= distSqRight) {
				if (isLeftLegForeign) {
					continue;
				}

				line.direction = leftLegDirection;
				line.point = leftCutoff + radius_ * invTimeHorizonObst * Vector2(-line.direction.y(), line.direction.x());
				orcaLines_.push_back(line);
				continue;
			}
			else {
				if (isRightLegForeign) {
					continue;
				}

				line.direction = -rightLegDirection;
				line.point = rightCutoff + radius_ * invTimeHorizonObst * Vector2(-line.direction.y(), line.direction.x());
				orcaLines_.push_back(line);
				continue;
			}
		}

		const size_t numObstLines = orcaLines_.size();

		const Fix16 invTimeHorizon = Fix16::one / timeHorizon_;

		for (size_t i = 0; i < agentNeighbors_.size(); ++i) {
			const Agent *const other = agentNeighbors_[i].second;

#pragma region Mass
			Fix16 totalMass = other->mass_ + mass_;
			Fix16 massRatio = other->mass_ / totalMass;
			Fix16 neighborMassRatio = mass_ / totalMass;

			Vector2 velocityOpt = massRatio >= Fix16::half ? (velocity_ - massRatio * velocity_) * Fix16::two : 
				prefVelocity_ + (velocity_ - prefVelocity_) * massRatio * Fix16::two;
			Vector2 neighborVelocityOpt = neighborMassRatio >= Fix16::half ? Fix16::two * other->velocity_ * (Fix16::one - neighborMassRatio) :
				other->prefVelocity_ + (other->velocity_ - other->prefVelocity_) * neighborMassRatio * Fix16::two;
			const Vector2 relativeVelocity = velocityOpt - neighborVelocityOpt;
#pragma endregion Mass

			const Vector2 relativePosition = other->position_ - position_;
			//const Vector2 relativeVelocity = velocity_ - other->velocity_;
			const Fix16 distSq = absSq(relativePosition);
			const Fix16 combinedRadius = radius_ + other->radius_;
			const Fix16 combinedRadiusSq = sqr(combinedRadius);

			Line line;
			Vector2 u;

			if (distSq > combinedRadiusSq) {
				const Vector2 w = relativeVelocity - invTimeHorizon * relativePosition;
				const Fix16 wLengthSq = absSq(w);

				const Fix16 dotProduct1 = w * relativePosition;

				if (dotProduct1 < Fix16::zero && sqr(dotProduct1) > combinedRadiusSq * wLengthSq) {
					const Fix16 wLength = (wLengthSq).sqrt();
					const Vector2 unitW = w / wLength;

					line.direction = Vector2(unitW.y(), -unitW.x());
					u = (combinedRadius * invTimeHorizon - wLength) * unitW;
				}
				else {
					const Fix16 leg = (distSq - combinedRadiusSq).sqrt();

					if (det(relativePosition, w) > Fix16::zero) {
						line.direction = Vector2(relativePosition.x() * leg - relativePosition.y() * combinedRadius, relativePosition.x() * combinedRadius + relativePosition.y() * leg) / distSq;
					}
					else {
						line.direction = -Vector2(relativePosition.x() * leg + relativePosition.y() * combinedRadius, -relativePosition.x() * combinedRadius + relativePosition.y() * leg) / distSq;
					}

					const Fix16 dotProduct2 = relativeVelocity * line.direction;

					u = dotProduct2 * line.direction - relativeVelocity;
				}
			}
			else {
				const Fix16 invTimeStep = Fix16::one / sim_->timeStep_;

				const Vector2 w = relativeVelocity - invTimeStep * relativePosition;

				const Fix16 wLength = abs(w);
				 Vector2 unitW = w / wLength;

				if (unitW.x().value == fix16_minimum || unitW.y() == fix16_minimum)
					unitW = Vector2(Fix16::zero, Fix16::zero);

				line.direction = Vector2(unitW.y(), -unitW.x());
				u = (combinedRadius * invTimeStep - wLength) * unitW;
			}

			//line.point = velocity_ + Fix16::half * u;
			line.point = velocityOpt + massRatio * u;
			orcaLines_.push_back(line);
		}

		size_t lineFail = linearProgram2(orcaLines_, maxSpeed_, prefVelocity_, false, newVelocity_);

		if (lineFail < orcaLines_.size()) {
			linearProgram3(orcaLines_, numObstLines, lineFail, maxSpeed_, newVelocity_);
		}

		//if (std::isnan(newVelocity_.x()))
		//{
		//	int k = 0;
		//}
	}

	void Agent::insertAgentNeighbor(const Agent *agent, Fix16 &rangeSq)
	{
        if ((this->m_MonsterType == MonsterType::NormalGround &&
             agent->m_MonsterType == MonsterType::SpecialGround) ||
            (this->m_MonsterType == MonsterType::SpecialGround &&
             agent->m_MonsterType == MonsterType::NormalGround) ||
            (this->m_MonsterType == MonsterType::Hero &&
             agent->m_MonsterType == MonsterType::NormalGround))
            return;

		if (this->m_MonsterType == MonsterType::IgnoreAll || agent->m_MonsterType == MonsterType::IgnoreAll)
		{
			return;
		}
        
		if (this != agent) {
			const Fix16 distSq = absSq(position_ - agent->position_);

			if (distSq < rangeSq) {
				if (agentNeighbors_.size() < maxNeighbors_) {
					agentNeighbors_.push_back(std::make_pair(distSq, agent));
				}

				size_t i = agentNeighbors_.size() - 1;

				while (i != 0 && distSq < agentNeighbors_[i - 1].first) {
					agentNeighbors_[i] = agentNeighbors_[i - 1];
					--i;
				}

				agentNeighbors_[i] = std::make_pair(distSq, agent);

				if (agentNeighbors_.size() == maxNeighbors_) {
					rangeSq = agentNeighbors_.back().first;
				}
			}
		}
	}

	void Agent::insertObstacleNeighbor(const Obstacle *obstacle, Fix16 rangeSq)
	{
		const Obstacle *const nextObstacle = obstacle->nextObstacle_;

		const Fix16 distSq = distSqPointLineSegment(obstacle->point_, nextObstacle->point_, position_);

		if (distSq < rangeSq) {
			obstacleNeighbors_.push_back(std::make_pair(distSq, obstacle));

			size_t i = obstacleNeighbors_.size() - 1;

			while (i != 0 && distSq < obstacleNeighbors_[i - 1].first) {
				obstacleNeighbors_[i] = obstacleNeighbors_[i - 1];
				--i;
			}

			obstacleNeighbors_[i] = std::make_pair(distSq, obstacle);
		}
	}

	void Agent::update()
	{
		velocity_ = newVelocity_;
		position_ += velocity_ * sim_->timeStep_;
	}

    void Agent::SetMonsterType(MonsterType monsterType)
    {
        m_MonsterType = monsterType;
    }

    MonsterType Agent::GetMonsterType()
    {
        return m_MonsterType;
    }

	bool linearProgram1(const std::vector<Line> &lines, size_t lineNo, Fix16 radius, const Vector2 &optVelocity, bool directionOpt, Vector2 &result)
	{
		const Fix16 dotProduct = lines[lineNo].point * lines[lineNo].direction;
		const Fix16 discriminant = sqr(dotProduct) + sqr(radius) - absSq(lines[lineNo].point);

		if (discriminant < Fix16::zero) {
			return false;
		}

		const Fix16 sqrtDiscriminant = (discriminant).sqrt();
		Fix16 tLeft = -dotProduct - sqrtDiscriminant;
		Fix16 tRight = -dotProduct + sqrtDiscriminant;

		for (size_t i = 0; i < lineNo; ++i) {
			const Fix16 denominator = det(lines[lineNo].direction, lines[i].direction);
			const Fix16 numerator = det(lines[i].direction, lines[lineNo].point - lines[i].point);

			if (Fix16::Abs(denominator) <= Fix16::epsilon) {
				if (numerator < Fix16::zero) {
					return false;
				}
				else {
					continue;
				}
			}

			const Fix16 t = numerator / denominator;

			if (denominator >= Fix16::zero) {
				tRight = std::min(tRight, t);
			}
			else {
				tLeft = std::max(tLeft, t);
			}

			if (tLeft > tRight) {
				return false;
			}
		}

		if (directionOpt) {
			if (optVelocity * lines[lineNo].direction > Fix16::zero) {
				result = lines[lineNo].point + tRight * lines[lineNo].direction;
			}
			else {
				result = lines[lineNo].point + tLeft * lines[lineNo].direction;
			}
		}
		else {
			const Fix16 t = lines[lineNo].direction * (optVelocity - lines[lineNo].point);

			if (t < tLeft) {
				result = lines[lineNo].point + tLeft * lines[lineNo].direction;
			}
			else if (t > tRight) {
				result = lines[lineNo].point + tRight * lines[lineNo].direction;
			}
			else {
				result = lines[lineNo].point + t * lines[lineNo].direction;
			}
		}

		return true;
	}

	size_t linearProgram2(const std::vector<Line> &lines, Fix16 radius, const Vector2 &optVelocity, bool directionOpt, Vector2 &result)
	{
		if (directionOpt) {
			result = optVelocity * radius;
		}
		else if (absSq(optVelocity) > sqr(radius)) {
			result = normalize(optVelocity) * radius;
		}
		else {
			result = optVelocity;
		}

		for (size_t i = 0; i < lines.size(); ++i) {
			if (det(lines[i].direction, lines[i].point - result) > Fix16::zero) {
				const Vector2 tempResult = result;

				if (!linearProgram1(lines, i, radius, optVelocity, directionOpt, result)) {
					result = tempResult;
					return i;
				}
			}
		}

		return lines.size();
	}

	void linearProgram3(const std::vector<Line> &lines, size_t numObstLines, size_t beginLine, Fix16 radius, Vector2 &result)
	{
		Fix16 distance = Fix16::zero;

		for (size_t i = beginLine; i < lines.size(); ++i) {
			if (det(lines[i].direction, lines[i].point - result) > distance) {
				std::vector<Line> projLines(lines.begin(), lines.begin() + static_cast<ptrdiff_t>(numObstLines));

				for (size_t j = numObstLines; j < i; ++j) {
					Line line;

					Fix16 determinant = det(lines[i].direction, lines[j].direction);

					if (Fix16::Abs(determinant) <= Fix16::epsilon) {
						if (lines[i].direction * lines[j].direction > Fix16::zero) {
							continue;
						}
						else {
							line.point = Fix16::half * (lines[i].point + lines[j].point);
						}
					}
					else {
						line.point = lines[i].point + (det(lines[j].direction, lines[i].point - lines[j].point) / determinant) * lines[i].direction;
					}

					line.direction = normalize(lines[j].direction - lines[i].direction);
					projLines.push_back(line);
				}

				const Vector2 tempResult = result;

				if (linearProgram2(projLines, radius, Vector2(-lines[i].direction.y(), lines[i].direction.x()), true, result) < projLines.size()) {
					result = tempResult;
				}

				distance = det(lines[i].direction, lines[i].point - result);
			}
		}
	}
}
