public class RockLockGemSkill : IceLockGemSkill
{
	protected override string GetSkinNameFor(MatchItem gem)
	{
		return gem.cell.cellType.ToString();
	}

//	protected override void PlaySoundActivate()
//	{
//		SoundManager.intance.PlaySound(SoundName.CAST_SKILL_ROCK);
//	}

//	protected override void PlaySoundDeactivate()
//	{
//		SoundManager.intance.PlaySound(SoundName.ROCK_SKILL_DEACTIVE);
//	}
}