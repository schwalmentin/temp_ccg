using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EventManager : NetworkSingleton<EventManager>
{
    #region Delegates

    public delegate void p_CardArrayDelegate(Card[] cards, ServerRpcParams serverRpcParams);
    public delegate void s_CardArrayDelegate(Card[] cards);

    public delegate void p_PlayedCardArrayDelegate(PlayedCard[] playedCards, ServerRpcParams serverRpcParams);
    public delegate void s_PlayedCardArrayDelegate(PlayedCard[] playedCards);

    public delegate void p_IntDelegate(int number, ServerRpcParams serverRpcParams);
    public delegate void s_IntDelegate(int number);

    public delegate void s_IntCardDelegate(int number, Card card);

    public delegate void p_CardDelegate(Card card, ServerRpcParams serverRpcParams);
    public delegate void s_CardDelegate(Card card);

    public delegate void p_EmptyDelegate(ServerRpcParams serverRpcParams);
    public delegate void s_EmptyDelegate();

    public delegate void p_BoolDelegate(bool condition, ServerRpcParams serverRpcParams);
    public delegate void s_BoolDelegate(bool condition);

    public delegate void s_BoolCardCardCardDelegate(bool condition, Card card, Card card2, Card card3);
    public delegate void p_CardArrayBoolDelegate(Card[] card, bool condition, ServerRpcParams serverRpcParams);
    public delegate void s_CardIntIntDelegate(Card card, int number, int number2);

    #endregion

    #region Player Events

    public event p_PlayedCardArrayDelegate p_passTurnDeploymentEvent;
    public event p_CardArrayBoolDelegate p_joinMatchEvent;
    public event p_IntDelegate p_ChooseLaneToAttackEvent;
    public event p_CardDelegate p_combatEvent;
    public event p_EmptyDelegate p_byPassGuardianEvent;
    public event p_BoolDelegate p_chooseInteractionEvent;

    #endregion

    #region Server Events

    public event s_PlayedCardArrayDelegate s_endTurnDeploymentEvent;
    public event s_CardArrayDelegate s_joinedMatchEvent;
    public event s_IntCardDelegate s_informAboutLaneEvent;
    public event s_BoolCardCardCardDelegate s_informCombatEvent;
    public event s_EmptyDelegate s_informByPassGuardianEvent;
    public event s_BoolDelegate s_endCombatEvent;
    public event s_CardIntIntDelegate s_endTurnCombatEvent;
    public event s_BoolDelegate s_matchResult;

    #endregion

    #region Server RPCs

    [ServerRpc(RequireOwnership = false)]
    public void JoinMatchServerRpc(Card[] deck, bool isInvader, ServerRpcParams serverRpcParams = default)
    {
        this.p_joinMatchEvent.Invoke(deck, isInvader, serverRpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PassTurnDeploymentServerRpc(PlayedCard[] playedCards, ServerRpcParams serverRpcParams = default)
    {
        this.p_passTurnDeploymentEvent.Invoke(playedCards, serverRpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChooseLaneToAttackServerRpc(int laneToAttack, ServerRpcParams serverRpcParams = default)
    {
        this.p_ChooseLaneToAttackEvent.Invoke(laneToAttack, serverRpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    public void CombatServerRpc(Card nightmare, ServerRpcParams serverRpcParams = default)
    {
        this.p_combatEvent.Invoke(nightmare, serverRpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ByPassGuardianServerRpc(ServerRpcParams serverRpcParams = default)
    {
        this.p_byPassGuardianEvent.Invoke(serverRpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChooseInteractionServerRpc(bool interactWithHiddenCard, ServerRpcParams serverRpcParams = default)
    {
        this.p_chooseInteractionEvent.Invoke(interactWithHiddenCard, serverRpcParams);
    }

    #endregion

    #region Client RPCs

    [ClientRpc]
    public void JoinedMatchClientRpc(Card[] startingHand, ClientRpcParams clientRpcParams)
    {
        this.s_joinedMatchEvent.Invoke(startingHand);
    }

    [ClientRpc]
    public void EndTurnDeploymentClientRpc(PlayedCard[] opponentPlayedCards, ClientRpcParams clientRpcParams)
    {
        this.s_endTurnDeploymentEvent.Invoke(opponentPlayedCards);
    }

    [ClientRpc]
    public void InformAboutLaneClientRpc(int attackedLane, Card guard, ClientRpcParams clientRpcParams = default)
    {
        this.s_informAboutLaneEvent.Invoke(attackedLane, guard);
    }

    [ClientRpc]
    public void InformCombatClientRpc(bool hasAttacked, Card nightmare, Card attackedGuard, Card newGuard, ClientRpcParams clientRpcParams = default)
    {
        this.s_informCombatEvent.Invoke(hasAttacked, nightmare, attackedGuard, newGuard);
    }

    [ClientRpc]
    public void EndCombatClientRpc(bool successfulAttack, ClientRpcParams clientRpcParams = default)
    {
        this.s_endCombatEvent.Invoke(successfulAttack);
    }

    [ClientRpc]
    public void EndTurnCombatClientRpc(Card cardToDraw, int earnedAttackerPoints, int earnedDefenderPoints, ClientRpcParams clientRpcParams = default)
    {
        this.s_endTurnCombatEvent.Invoke(cardToDraw, earnedAttackerPoints, earnedDefenderPoints);
    }

    [ClientRpc]
    public void MatchResultClientRpc(bool hasWon, ClientRpcParams clientRpcParams)
    {
        this.s_matchResult.Invoke(hasWon);
    }

    #endregion

    #region Debug RPCs

    [ServerRpc(RequireOwnership = false)]
    public void LogMessageServerRpc(string message)
    {
        Debug.Log("CLIENT: " + message);
    }

    [ClientRpc]
    public void LogMessageClientRpc(string message)
    {
        Debug.Log("SERVER: " + message);
    }


    #endregion
}
