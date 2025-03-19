namespace HutongGames.PlayMaker.Actions;

[ActionCategory("Pegasus")]
[Tooltip("Modify the stats/appearance of a display actor.  This should only be used for cards spawned in fx as actual game entities are managed by the server.")]
public class ModifyActorAction : FsmStateAction
{
	[Tooltip("Actor to modify.")]
	[UIHint(UIHint.Variable)]
	[RequiredField]
	public FsmGameObject m_ActorObject;

	[Tooltip("How much attack the actor should have.")]
	public FsmInt m_attack;

	[Tooltip("How much health the actor should have.")]
	public FsmInt m_health;

	[Tooltip("If true, modify attack.")]
	public bool m_modifyAttack;

	[Tooltip("If true, modify health.")]
	public bool m_modifyHealth;

	[Tooltip("If true, show the play actor with taunt.")]
	public FsmBool m_useTaunt;

	[Tooltip("If true, add attack and health to existing stats instead of setting.")]
	public bool m_addStats;

	public override void Reset()
	{
		m_ActorObject = null;
		m_attack = 0;
		m_health = 0;
		m_useTaunt = false;
		m_addStats = false;
	}

	public override void OnEnter()
	{
		if (m_ActorObject.Value == null)
		{
			return;
		}
		Actor actor = m_ActorObject.Value.GetComponent<Actor>();
		if (actor == null)
		{
			return;
		}
		UberText attackText = actor.GetAttackText();
		UberText healthText = actor.GetHealthText();
		if (m_addStats)
		{
			int.TryParse(attackText.Text, out var currentAttack);
			int.TryParse(healthText.Text, out var currentHealth);
			if (m_modifyAttack)
			{
				attackText.Text = (currentAttack + m_attack.Value).ToString() ?? "";
			}
			if (m_modifyHealth)
			{
				healthText.Text = (currentHealth + m_health.Value).ToString() ?? "";
			}
		}
		else
		{
			if (m_modifyAttack)
			{
				attackText.Text = m_attack.Value.ToString() ?? "";
			}
			if (m_modifyHealth)
			{
				healthText.Text = m_health.Value.ToString() ?? "";
			}
		}
		if (m_useTaunt.Value)
		{
			actor.ActivateTaunt();
		}
		Finish();
	}
}
