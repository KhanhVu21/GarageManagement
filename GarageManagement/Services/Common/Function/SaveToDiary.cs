using GarageManagement.Data.Entity;
using GarageManagement.Services.Common.Model;
using GarageManagement.Services.Dtos;
using Mapster;

namespace GarageManagement.Services.Common.Function
{
    public class SaveToDiary
    {
        #region METHOD
        public Diary InsertDiary(SaveDiaryModel saveDiaryModel)
        {
            Diary diary = new Diary();
            DiaryDto diaryDto = new DiaryDto();

            if (saveDiaryModel.Operation == "Create")
            {
                diaryDto = new DiaryDto()
                {
                    Id = Guid.NewGuid(),
                    DateCreate = DateTime.Now,
                    UserId = saveDiaryModel.IdUserCurrent,
                    WithId = saveDiaryModel.IdWith,
                    UserName = saveDiaryModel.Fullname,
                    Content = $"{saveDiaryModel.Fullname} đã thêm mới {saveDiaryModel.Table} - {saveDiaryModel.Content}",
                    Operation = saveDiaryModel.Operation,
                    Title = "Thêm mới vào CSDL",
                    IsSuccess = saveDiaryModel.IsSuccess,
                    Table = saveDiaryModel.Table
                };
            }
            if (saveDiaryModel.Operation == "Update")
            {
                diaryDto = new DiaryDto()
                {
                    Id = Guid.NewGuid(),
                    DateCreate = DateTime.Now,
                    UserId = saveDiaryModel.IdUserCurrent,
                    WithId = saveDiaryModel.IdWith,
                    UserName = saveDiaryModel.Fullname,
                    Content = $"{saveDiaryModel.Fullname} đã cập nhật {saveDiaryModel.Table} - {saveDiaryModel.Content}",
                    Operation = saveDiaryModel.Operation,
                    Title = "Cập nhật vào CSDL",
                    IsSuccess = saveDiaryModel.IsSuccess,
                    Table = saveDiaryModel.Table
                };
            }
            if (saveDiaryModel.Operation == "Delete")
            {
                diaryDto = new DiaryDto()
                {
                    Id = Guid.NewGuid(),
                    DateCreate = DateTime.Now,
                    UserId = saveDiaryModel.IdUserCurrent,
                    WithId = saveDiaryModel.IdWith,
                    UserName = saveDiaryModel.Fullname,
                    Content = $"{saveDiaryModel.Fullname} đã xóa {saveDiaryModel.Table} - {saveDiaryModel.Content}",
                    Operation = saveDiaryModel.Operation,
                    Title = "Xóa thông tin CSDL",
                    IsSuccess = saveDiaryModel.IsSuccess,
                    Table = saveDiaryModel.Table
                };
            }

            diary = diaryDto.Adapt<Diary>();
            return diary;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="saveDiaryModel"></param>
        /// <param name="IdWith"></param>
        /// <param name="Operation"></param>
        /// <param name="Table"></param>
        /// <param name="Content"></param>
        /// <returns></returns>
        public Diary ConvertDriaryToListDiary(SaveDiaryModel saveDiaryModel, Guid IdWith, string Operation, string Table, string Content)
        {
            var diaryModel = new SaveDiaryModel();
            diaryModel = saveDiaryModel.Adapt<SaveDiaryModel>();
            diaryModel.IdWith = IdWith;
            diaryModel.Operation = Operation;
            diaryModel.Table = Table;
            diaryModel.Content = Content;

            Diary diarySave = InsertDiary(diaryModel);
            return diarySave;
        }
        #endregion
    }
}
