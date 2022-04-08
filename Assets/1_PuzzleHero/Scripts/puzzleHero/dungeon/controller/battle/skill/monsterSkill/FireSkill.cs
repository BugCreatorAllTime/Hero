public class FireSkill : PoisonSkill
{
	protected override string GetSkinNameFor(MatchItem gem) {
		return gem.cell.cellType.ToString();
	}

//	protected override void PlaySoundActivate()
//	{
//		SoundManager.intance.PlaySound (SoundName.CAST_SKILL_FIRE);
//	}
}