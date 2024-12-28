export interface UserDetail{
    Id: string;
    fullName: string;
    email: string;
    roles: string[];
    phoneNumber: string;
    twoFactorEnabled: true;
    phoneNumberConfirmed: true;
    accessFailedCount:0;
}