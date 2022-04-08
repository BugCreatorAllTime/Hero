using UnityEngine;
using System.Collections;

public class UpdateDataScrollView : UIWrapContent
{

	protected override void onInit()
	{
		//base.onInit();
	}
	protected override bool UpdateItem(Transform item, int index, bool forward)
	{
		bool update = true;
		if (forward)
		{
			int i = (index == mChildren.size - 1) ? 0 : index + 1;
			int id = mChildren[i].GetComponent<MapPieceContent>().indexMap - 1;
			if (mChildren[i].GetComponent<MapPieceContent>().indexMap >= 1)
			{
//				this.WrapBottom = true;
				MapPieceContent pack = mChildren[index].GetComponent<MapPieceContent>();
				pack.UpdateData(id, 2);
			}
			else
			{
//				this.WrapTop = false;
				update = false;
			}
		}
		else
		{
			int i = (index == 0) ? mChildren.size - 1 : index - 1;
			int id = mChildren[i].GetComponent<MapPieceContent>().indexMap + 1;
			if (mChildren[i].GetComponent<MapPieceContent>().indexMap < 7)
			{
//				this.WrapTop = true;
				MapPieceContent pack = mChildren[index].GetComponent<MapPieceContent>();
				pack.UpdateData(id, -2);
			}
			else
			{
//				this.WrapBottom = false;
				update = false;
			}
		}
		return update;
	}

}
