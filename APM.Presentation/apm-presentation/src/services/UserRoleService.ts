import type { User } from "@/interfaces/DTOEntities";
import $ from "../utils/request";
import type { UsualApiData } from "@/interfaces/HttpReponse";

class PlayerService {
    UserLogin(user: User): Promise<UsualApiData<string>> {
        return $.post(`User/UserLogin`, user);
    }

    CheckUserToken(): Promise<UsualApiData<boolean>> {
        return $.get('User/CheckUserToken')
    }

    GetCurrentUser(): Promise<UsualApiData<User>> {
        return $.get('User/GetCurrentUser');
    }
}

export default new PlayerService();