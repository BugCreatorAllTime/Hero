public class Effects
{
	public const string match = "Animation/Fx/Gems/Match/Match.ske";
	public const string destroy = "Animation/Fx/Gems/Destroy/{0}/{1}.ske";
	public const string transform = "Animation/Fx/Transform/Transform.ske";
	public const string resurrect = "Animation/Fx/Resurrect/Resurrect.ske";
	public const string charge = "Animation/Fx/Charge/Charge.ske";
	public const string poison = "Animation/Fx/Poison/Poison.ske";
	public const string ice = "Animation/Fx/Ice/Ice.ske";
	public const string slashVertically = "Animation/Fx/Slash/Vertical/Vertical.ske";
	public const string slashHorizontally = "Animation/Fx/Slash/Horizontal/Horizontal.ske";
	public const string adventurer = "Animation/Fx/ItemSetSkills/Adventurer/Adventurer.ske";
	public const string bronze = "Animation/Fx/ItemSetSkills/Bronze/Bronze.ske";
	public const string beHitCharacter = "Animation/Fx/BeHit/Character/Character.ske";
	public const string beHitMonster = "Animation/Fx/BeHit/Monster/Monster.ske";
	public const string match4Explosion = "Animation/Fx/Match/4/4.ske";
	public const string hunter = "Animation/Fx/ItemSetSkills/Hunter/Hunter.ske";
	public const string elven = "Animation/Fx/ItemSetSkills/Elven/Elven.ske";
	public const string knight = "Animation/Fx/ItemSetSkills/Knight/Knight.ske";
	public const string samurai = "Animation/Fx/ItemSetSkills/Samurai/Samurai.ske";
	public const string viking = "Animation/Fx/ItemSetSkills/Viking/Viking.ske";
	public const string vikingCharge = "Animation/Fx/ItemSetSkills/VikingBegin/VikingBegin.ske";
	public const string paladin = "Animation/Fx/ItemSetSkills/Paladin/Paladin.ske";
	public const string counter = "Animation/Fx/Counter/Counter.ske";
	public const string swap = "Animation/Fx/Swap/Swap.ske";

	public static string Setup(string pathToAsset, string skillName)
	{
		return string.Format(pathToAsset, new[] {skillName, skillName});
	}
}